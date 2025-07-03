#define GRID_DEBUG

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace DotsRTS
{
    public partial struct GridSystem : ISystem
    {
        public const int WALL_COST = byte.MaxValue;
        public const int HEAVY_COST = 50;
        public const int FLOW_FIELD_MAP_COUNT = 100;


        public struct GridSystemData: IComponentData
        {
            public int width;
            public int height;
            public float gridNodeSize;
            public NativeArray<GridMap> gridMap;
            public int nextGridIndex;
            public NativeArray<byte> costMap;
        }

        public struct GridNode: IComponentData
        {
            public int index;
            public int x;
            public int y;
            public byte cost;
            public int bestCost;
            public float2 vector;
        }

        public struct GridMap
        {
            public NativeArray<Entity> gridEntities;
            public int2 targetGridPos;
            public bool isValid;
        }

#if !GRID_DEBUG
        [BurstCompile]
#endif
        public void OnCreate(ref SystemState state)
        {
            int width = 20;
            int height = 10;
            float gridNodeSize = 5f;
            int totalCount = width * height;

            Entity gridNodePrefab = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<GridNode>(gridNodePrefab);

            NativeArray<GridMap> gridMaps = new NativeArray<GridMap>(FLOW_FIELD_MAP_COUNT, Allocator.Persistent);
            for (int i = 0; i < FLOW_FIELD_MAP_COUNT; ++i)
            {
                GridMap gridMap = new GridMap();
                gridMap.isValid = false;
                gridMap.gridEntities = new NativeArray<Entity>(totalCount, Allocator.Persistent);

                state.EntityManager.Instantiate(gridNodePrefab, gridMap.gridEntities);
                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        int index = CalculateIndex(x, y, width);
                        GridNode node = new GridNode
                        {
                            index = index,
                            x = x,
                            y = y,
                        };
#if GRID_DEBUG
                        state.EntityManager.SetName(gridMap.gridEntities[index], "GridNode_" + x + "_" + y);
#endif
                        SystemAPI.SetComponent(gridMap.gridEntities[index], node);
                    }
                }
                gridMaps[i] = gridMap;
            }
            state.EntityManager.AddComponent<GridSystemData>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new GridSystemData
            {
                width = width,
                height = height,
                gridNodeSize = gridNodeSize,
                gridMap = gridMaps,
                costMap = new NativeArray<byte>(totalCount, Allocator.Persistent)
            });
        }

#if !GRID_DEBUG
        [BurstCompile]
#endif
        public void OnUpdate(ref SystemState state)
        {
            GridSystemData data = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);

            foreach (var (request, enableRequest, follower, enableFollower) in SystemAPI.Query<RefRW<FlowFieldPathRequest>, EnabledRefRW<FlowFieldPathRequest>, RefRW<FlowFieldFollower>, EnabledRefRW<FlowFieldFollower>>().WithPresent<FlowFieldFollower>())
            {
                int2 targetGridPosition = GetGridPosition(request.ValueRO.targetPos, data.gridNodeSize);

                bool foundPath = false;
                for(int index = 0; index < FLOW_FIELD_MAP_COUNT; ++index)
                {
                    if (data.gridMap[index].isValid && data.gridMap[index].targetGridPos.Equals(targetGridPosition))
                    {
                        follower.ValueRW.gridIndex = index;
                        follower.ValueRW.targetPos = request.ValueRW.targetPos;
                        enableFollower.ValueRW = true;
                        foundPath = true;
                    }
                }
                if (!foundPath)
                {
                    int gridIndex = data.nextGridIndex;
                    data.nextGridIndex++;
                    if (data.nextGridIndex >= FLOW_FIELD_MAP_COUNT)
                        data.nextGridIndex = 0;
                    SystemAPI.SetComponent(state.SystemHandle, data);

                    FlowFieldPathfinding(ref state, targetGridPosition, data, gridIndex);

                    follower.ValueRW.gridIndex = gridIndex;
                    follower.ValueRW.targetPos = request.ValueRW.targetPos;
                    enableRequest.ValueRW = false;
                    enableFollower.ValueRW = true;
                }
            }

#if GRID_DEBUG
            GridSystemDebug.Instance?.InitializeGrid(data);
            GridSystemDebug.Instance?.UpdateGrid(data);
#endif
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            var data = SystemAPI.GetComponentRW<GridSystemData>(state.SystemHandle);
            for (int i = 0; i < FLOW_FIELD_MAP_COUNT; ++i)
                data.ValueRW.gridMap[i].gridEntities.Dispose();
            data.ValueRW.gridMap.Dispose();
            data.ValueRW.costMap.Dispose();
        }

        private void FlowFieldPathfinding(ref SystemState state, int2 targetPos, GridSystemData data, int gridIndex)
        {
            NativeArray<RefRW<GridNode>> gridNodeArray = new NativeArray<RefRW<GridNode>>(data.width * data.height, Allocator.Temp);

            //Initialize nodes
            for (int x = 0; x < data.width; ++x)
            {
                for (int y = 0; y < data.height; ++y)
                {
                    int index = CalculateIndex(x, y, data.width);
                    Entity entity = data.gridMap[gridIndex].gridEntities[index];

                    var gridNode = SystemAPI.GetComponentRW<GridNode>(entity);
                    gridNodeArray[index] = gridNode;

                    gridNode.ValueRW.vector = new float2(0, 1);
                    if (x == targetPos.x && y == targetPos.y)
                    {
                        gridNode.ValueRW.cost = 0;
                        gridNode.ValueRW.bestCost = 0;
                    }
                    else
                    {
                        gridNode.ValueRW.cost = 1;
                        gridNode.ValueRW.bestCost = int.MaxValue;
                    }
                }
            }

            //Find walls
            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;

            NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);
            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.PATHFINDING_WALLS,
                GroupIndex = 0
            }; 
            var filterHeavy = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.PATHFINDING_HEAVY,
                GroupIndex = 0
            };
            for (int x = 0; x < data.width; ++x)
            {
                for (int y = 0; y < data.height; ++y)
                {
                    var pos = GetWorldCenterPosition(x, y, data.gridNodeSize);
                    if (collision.OverlapSphere(pos, data.gridNodeSize * 0.5f, ref distanceHits, filter))
                    {
                        int index = CalculateIndex(x, y, data.width);
                        gridNodeArray[index].ValueRW.cost = WALL_COST;
                        data.costMap[index] = WALL_COST;
                    }
                    if (collision.OverlapSphere(pos, data.gridNodeSize * 0.5f, ref distanceHits, filterHeavy))
                    {
                        int index = CalculateIndex(x, y, data.width);
                        gridNodeArray[index].ValueRW.cost = HEAVY_COST;
                        data.costMap[index] = HEAVY_COST;
                    }
                }
            }
            distanceHits.Dispose();

            //Calculate costs
            NativeQueue<RefRW<GridNode>> gridQueue = new NativeQueue<RefRW<GridNode>>(Allocator.Temp);
            var targetNode = gridNodeArray[CalculateIndex(targetPos, data.width)];
            gridQueue.Enqueue(targetNode);

            while (gridQueue.Count > 0)
            {
                var currentNode = gridQueue.Dequeue();

                var neighbours = GetNeighbours(currentNode, gridNodeArray, data.width, data.height);
                foreach (var node in neighbours)
                {
                    //if (node.ValueRO.cost == WALL_COST)
                    //    continue;

                    int newBestCost = currentNode.ValueRO.bestCost + node.ValueRO.cost;
                    if (newBestCost < node.ValueRO.bestCost)
                    {
                        node.ValueRW.bestCost = newBestCost;
                        node.ValueRW.vector = CalculateVector(node.ValueRO.x, node.ValueRO.y, currentNode.ValueRO.x, currentNode.ValueRO.y);
                        gridQueue.Enqueue(node);
                    }
                }
                neighbours.Dispose();
            }
            gridQueue.Dispose();
            gridNodeArray.Dispose();

            GridMap gridMap = data.gridMap[gridIndex];
            gridMap.targetGridPos = targetPos;
            gridMap.isValid = true;
            data.gridMap[gridIndex] = gridMap;
            SystemAPI.SetComponent(state.SystemHandle, data);
        }

        public static int CalculateIndex(int x, int y, int width)
        {
            return x + y * width;
        }
        public static int CalculateIndex(int2 pos, int width)
        {
            return CalculateIndex(pos.x, pos.y, width);
        }

        public static float3 GetWorldPosition(int x, int y, float gridNodeSize)
        {
            return new float3(x * gridNodeSize, 0f, y * gridNodeSize);
        }
        public static float3 GetWorldCenterPosition(int x, int y, float gridNodeSize)
        {
            return new float3(x * gridNodeSize + gridNodeSize * 0.5f, 0f, y * gridNodeSize + gridNodeSize * 0.5f);
        }

        public static int2 GetGridPosition(float3 worldPosition, float gridNodeSize)
        {
            return new int2((int)math.floor(worldPosition.x / gridNodeSize), (int)math.floor(worldPosition.z / gridNodeSize));
        }

        public static bool IsValidGridPosition(int2 gridPos, int width, int height)
        {
            return gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < width && gridPos.y < height;
        }

        public static NativeList<RefRW<GridNode>> GetNeighbours(RefRW<GridNode> currentNode, NativeArray<RefRW<GridNode>> nodes, int width, int height)
        {
            NativeList<RefRW<GridNode>> neighbours = new NativeList<RefRW<GridNode>>(Allocator.Temp);

            int x = currentNode.ValueRO.x;
            int y = currentNode.ValueRO.y;
            int2 posLeft    = new int2(x - 1, y + 0);
            int2 posRight   = new int2(x + 1, y + 0);
            int2 posUp      = new int2(x + 0, y + 1);
            int2 posDown    = new int2(x + 0, y - 1);

            int2 posLowerLeft = new int2(x - 1, y - 1);
            int2 posLowerRight = new int2(x + 1, y - 1);
            int2 posUpperLeft = new int2(x - 1, y + 1);
            int2 posUpperRight = new int2(x + 1, y + 1);

            if (IsValidGridPosition(posLeft, width, height)) neighbours.Add(nodes[CalculateIndex(posLeft, width)]);
            if (IsValidGridPosition(posRight, width, height)) neighbours.Add(nodes[CalculateIndex(posRight, width)]);
            if (IsValidGridPosition(posUp, width, height)) neighbours.Add(nodes[CalculateIndex(posUp, width)]);
            if (IsValidGridPosition(posDown, width, height)) neighbours.Add(nodes[CalculateIndex(posDown, width)]);

            if (IsValidGridPosition(posLowerLeft, width, height)) neighbours.Add(nodes[CalculateIndex(posLowerLeft, width)]);
            if (IsValidGridPosition(posLowerRight, width, height)) neighbours.Add(nodes[CalculateIndex(posLowerRight, width)]);
            if (IsValidGridPosition(posUpperLeft, width, height)) neighbours.Add(nodes[CalculateIndex(posUpperLeft, width)]);
            if (IsValidGridPosition(posUpperRight, width, height)) neighbours.Add(nodes[CalculateIndex(posUpperRight, width)]);

            return neighbours;
        }

        public static float2 CalculateVector(int fromX, int fromY, int toX, int toY)
        {
            return new float2(toX, toY) - new float2(fromX, fromY);
        }

        public static float3 GetWorldMovementVector(float2 vector)
        {
            return new float3(vector.x, 0f, vector.y);
        }

        public static bool IsWall(GridNode node)
        {
            return node.cost == WALL_COST;
        }

        public static bool IsWall(int2 gridPos, GridSystemData data)
        {
            return data.costMap[CalculateIndex(gridPos, data.width)] == WALL_COST;
        }

        public static bool IsValidWalkablePosition(float3 worldPos, GridSystemData data)
        {
            int2 gridPos = GetGridPosition(worldPos, data.gridNodeSize);
            return IsValidGridPosition(gridPos, data.width, data.height) && !IsWall(gridPos, data);
        }
    }
}