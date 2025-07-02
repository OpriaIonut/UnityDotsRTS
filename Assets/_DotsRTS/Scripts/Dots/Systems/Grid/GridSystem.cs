#define GRID_DEBUG

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRTS
{
    public partial struct GridSystem : ISystem
    {
        public struct GridSystemData: IComponentData
        {
            public int width;
            public int height;
            public float gridNodeSize;
            public GridMap gridMap;
        }

        public struct GridNode: IComponentData
        {
            public int x;
            public int y;
            public byte data;
        }

        public struct GridMap
        {
            public NativeArray<Entity> gridEntities;
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

            GridMap gridMap = new GridMap();
            gridMap.gridEntities = new NativeArray<Entity>(totalCount, Allocator.Persistent);

            state.EntityManager.Instantiate(gridNodePrefab, gridMap.gridEntities);
            for(int x = 0; x < width; ++x)
            {
                for(int y = 0; y < height; ++y)
                {
                    int index = CalculateIndex(x, y, width);
                    GridNode node = new GridNode
                    {
                        x = x,
                        y = y,
                    };
#if GRID_DEBUG
                    state.EntityManager.SetName(gridMap.gridEntities[index], "GridNode_" + x + "_" + y);
#endif
                    SystemAPI.SetComponent(gridMap.gridEntities[index], node);
                }
            }

            state.EntityManager.AddComponent<GridSystemData>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new GridSystemData
            {
                width = width,
                height = height,
                gridNodeSize = gridNodeSize,
                gridMap = gridMap
            });
        }

#if !GRID_DEBUG
        [BurstCompile]
#endif
        public void OnUpdate(ref SystemState state)
        {
            GridSystemData data = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);

            if(Input.GetMouseButtonDown(0))
            {
                float3 mouseWorldPos = MouseWorldPosition.Instance.GetPosition();
                int2 mouseGridPos = GetGridPosition(mouseWorldPos, data.gridNodeSize);
                if (IsValidGridPosition(mouseGridPos, data.width, data.height))
                {
                    int index = CalculateIndex(mouseGridPos.x, mouseGridPos.y, data.width);
                    Entity gridNode = data.gridMap.gridEntities[index];
                    var node = SystemAPI.GetComponentRW<GridNode>(gridNode);
                    node.ValueRW.data = 1;
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
            data.ValueRW.gridMap.gridEntities.Dispose();
        }

        public static int CalculateIndex(int x, int y, int width)
        {
            return x + y * width;
        }

        public static float3 GetWorldPosition(int x, int y, float gridNodeSize)
        {
            return new float3(x * gridNodeSize, 0f, y * gridNodeSize);
        }

        public static int2 GetGridPosition(float3 worldPosition, float gridNodeSize)
        {
            return new int2((int)math.floor(worldPosition.x / gridNodeSize), (int)math.floor(worldPosition.z / gridNodeSize));
        }

        public static bool IsValidGridPosition(int2 gridPos, int width, int height)
        {
            return gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < width && gridPos.y < height;
        }
    }
}