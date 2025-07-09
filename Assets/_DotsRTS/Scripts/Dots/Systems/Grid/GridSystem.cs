//#define GRID_DEBUG

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using static DotsRTS.GridSystem;

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
            public NativeArray<Entity> totalGridMapEntityArray;
        }

        public struct GridNode: IComponentData
        {
            public int gridIndex;
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

        public ComponentLookup<GridNode> gridNodeLookup;

#if !GRID_DEBUG
        [BurstCompile]
#endif
        public void OnCreate(ref SystemState state)
        {
            int width = 40;
            int height = 40;
            float gridNodeSize = 5f;
            int totalCount = width * height;

            Entity gridNodePrefab = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<GridNode>(gridNodePrefab);

            NativeArray<GridMap> gridMaps = new NativeArray<GridMap>(FLOW_FIELD_MAP_COUNT, Allocator.Persistent);
            NativeList<Entity> totalGridMapEntities = new NativeList<Entity>(FLOW_FIELD_MAP_COUNT * totalCount, Allocator.Temp);
            for (int i = 0; i < FLOW_FIELD_MAP_COUNT; ++i)
            {
                GridMap gridMap = new GridMap();
                gridMap.isValid = false;
                gridMap.gridEntities = new NativeArray<Entity>(totalCount, Allocator.Persistent);

                state.EntityManager.Instantiate(gridNodePrefab, gridMap.gridEntities);
                totalGridMapEntities.AddRange(gridMap.gridEntities);

                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        int index = CalculateIndex(x, y, width);
                        GridNode node = new GridNode
                        {
                            gridIndex = i,
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
                costMap = new NativeArray<byte>(totalCount, Allocator.Persistent),
                totalGridMapEntityArray = totalGridMapEntities.ToArray(Allocator.Persistent)
            });
            totalGridMapEntities.Dispose();

            gridNodeLookup = SystemAPI.GetComponentLookup<GridNode>(false);
        }

#if !GRID_DEBUG
        [BurstCompile]
#endif
        public void OnUpdate(ref SystemState state)
        {
            GridSystemData data = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);

            gridNodeLookup.Update(ref state);

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
            data.ValueRW.totalGridMapEntityArray.Dispose();
        }

        private void FlowFieldPathfinding(ref SystemState state, int2 targetPos, GridSystemData data, int gridIndex)
        {
            NativeArray<RefRW<GridNode>> gridNodeArray = new NativeArray<RefRW<GridNode>>(data.width * data.height, Allocator.Temp);

            //Initialize nodes
            InitializeGridJob initializeJob = new InitializeGridJob
            {
                gridIndex = gridIndex,
                targetPos = targetPos,
            };
            JobHandle initializeJobHandle = initializeJob.ScheduleParallel(state.Dependency);
            initializeJobHandle.Complete();

            for(int x = 0; x < data.width; x++)
            {
                for(int y = 0; y < data.height; y++)
                {
                    int index = CalculateIndex(x, y, data.width);
                    Entity entity = data.gridMap[gridIndex].gridEntities[index];
                    var node = SystemAPI.GetComponentRW<GridNode>(entity);
                    gridNodeArray[index] = node;
                }
            }

            //Find walls
            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;

            UpdateCostMapJob updateCostJob = new UpdateCostMapJob
            {
                width = data.width,
                gridNodeSize = data.gridNodeSize,
                collision = collision,
                costMap = data.costMap,
                gridMap = data.gridMap[gridIndex],
                gridNodeLookup = gridNodeLookup,
                filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.PATHFINDING_WALLS,
                    GroupIndex = 0
                },
                filterHeavy = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.PATHFINDING_HEAVY,
                    GroupIndex = 0
                }
            };
            JobHandle updateCostJobHandle = updateCostJob.ScheduleParallel(data.width * data.height,50, state.Dependency);
            updateCostJobHandle.Complete();

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
                    if (node.ValueRO.cost == WALL_COST)
                        continue;

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
        public static int2 GetGridPositionFromIndex(int index, int width)
        {
            int y = (int)math.floor(index / width);
            int x = index % width;
            return new int2(x, y);
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

        public static bool IsWall(int2 gridPos, int width, NativeArray<byte> costMap)
        {
            return costMap[CalculateIndex(gridPos, width)] == WALL_COST;
        }

        public static bool IsValidWalkablePosition(float3 worldPos, GridSystemData data)
        {
            int2 gridPos = GetGridPosition(worldPos, data.gridNodeSize);
            return IsValidGridPosition(gridPos, data.width, data.height) && !IsWall(gridPos, data);
        }
        public static bool IsValidWalkablePosition(float3 worldPos, float gridNodeSize, int width, int height, NativeArray<byte> costMap)
        {
            int2 gridPos = GetGridPosition(worldPos, gridNodeSize);
            return IsValidGridPosition(gridPos, width, height) && !IsWall(gridPos, width, costMap);
        }
    }

    [BurstCompile]
    public partial struct InitializeGridJob: IJobEntity
    {
        [ReadOnly] public int gridIndex;
        [ReadOnly] public int2 targetPos;

        public void Execute(ref GridNode gridNode)
        {
            if (gridIndex != gridNode.gridIndex)
                return;

            gridNode.vector = new float2(0, 1);
            if (gridNode.x == targetPos.x && gridNode.y == targetPos.y)
            {
                gridNode.cost = 0;
                gridNode.bestCost = 0;
            }
            else
            {
                gridNode.cost = 1;
                gridNode.bestCost = int.MaxValue;
            }
        }
    }

    [BurstCompile]
    public partial struct UpdateCostMapJob: IJobFor
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<GridNode> gridNodeLookup;
        [NativeDisableParallelForRestriction] public NativeArray<byte> costMap;

        [ReadOnly] public int width;
        [ReadOnly] public float gridNodeSize;
        [ReadOnly] public CollisionWorld collision;
        [ReadOnly] public GridMap gridMap;
        [ReadOnly] public CollisionFilter filter;
        [ReadOnly] public CollisionFilter filterHeavy;

        public void Execute(int index)
        {
            NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);

            var gridPos = GetGridPositionFromIndex(index, width);
            var pos = GetWorldCenterPosition(gridPos.x, gridPos.y, gridNodeSize);
            if (collision.OverlapSphere(pos, gridNodeSize * 0.5f, ref distanceHits, filter))
            {
                GridNode node = gridNodeLookup[gridMap.gridEntities[index]];
                node.cost = WALL_COST;
                gridNodeLookup[gridMap.gridEntities[index]] = node;
                costMap[index] = WALL_COST;
            }
            if (collision.OverlapSphere(pos, gridNodeSize * 0.5f, ref distanceHits, filterHeavy))
            {
                GridNode node = gridNodeLookup[gridMap.gridEntities[index]];
                node.cost = HEAVY_COST;
                gridNodeLookup[gridMap.gridEntities[index]] = node;
                costMap[index] = HEAVY_COST;
            }
            distanceHits.Dispose();
        }
    }
}