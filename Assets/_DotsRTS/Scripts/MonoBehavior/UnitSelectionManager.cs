using System;
using System.Net.NetworkInformation;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

namespace DotsRTS
{
    public class UnitSelectionManager : MonoBehaviour
    {
        public UnityAction OnSelectionAreaStart;
        public UnityAction OnSelectionAreaEnd;

        [SerializeField] private float multipleSelectionRectPixelThreshold = 40f;
        [SerializeField] private float multiplePositionRingSize = 2.2f;
        [SerializeField] private int minRingPositions = 2;
        [SerializeField] private float extraPositionsPerRingMultiplier = 2.0f;


        private Vector2 selectionStartPos;


        #region Singleton
        public static UnitSelectionManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one singleton of type UnitSelectionManager in the scene; deleting from: " + gameObject.name);
                Destroy(this);
            }
            else
                Instance = this;
        }
        #endregion


        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                selectionStartPos = Input.mousePosition;
                OnSelectionAreaStart?.Invoke();
            }
            if (Input.GetMouseButtonUp(0))
            {
                Vector2 selectionEndPos = Input.mousePosition;
                Rect selectionRect = GetSelectionRect();
                float selectionAreaSize = selectionRect.width + selectionRect.height;
                bool isMultipleSelection = selectionAreaSize > multipleSelectionRectPixelThreshold;

                DeselectAllUnits();

                if (isMultipleSelection)
                    SelectUnitsInRect(selectionRect);
                else
                    SelectSingleUnit();

                OnSelectionAreaEnd?.Invoke();
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (SelectOverrideTargetZombie())
                    return;

                Vector3 mouseWorldPos = MouseWorldPosition.Instance.GetPosition();
                MoveSelectedUnitsToPosition(mouseWorldPos);
                HandleBarracksRallyPosition(mouseWorldPos);
            }
        }

        public Rect GetSelectionRect()
        {
            Vector2 selectionEndPos = Input.mousePosition;
            Vector2 lowerLeftCorner = new Vector2(Mathf.Min(selectionStartPos.x, selectionEndPos.x), Mathf.Min(selectionStartPos.y, selectionEndPos.y));
            Vector2 upperRightCorner = new Vector2(Mathf.Max(selectionStartPos.x, selectionEndPos.x), Mathf.Max(selectionStartPos.y, selectionEndPos.y));
            return new Rect(lowerLeftCorner.x, lowerLeftCorner.y, upperRightCorner.x - lowerLeftCorner.x, upperRightCorner.y - lowerLeftCorner.y);
        }

        private void DeselectAllUnits()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);

            NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedEntities = query.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int index = 0; index < entities.Length; ++index)
            {
                entityManager.SetComponentEnabled<Selected>(entities[index], false);
                Selected selected = selectedEntities[index];
                selected.onDeselected = true;
                entityManager.SetComponentData(entities[index], selected);
            }
        }

        private void SelectUnitsInRect(Rect selectionRect)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

            NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
            NativeArray<LocalTransform> movers = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            NativeArray<Selected> selectedEntities = query.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int index = 0; index < movers.Length; ++index)
            {
                LocalTransform mover = movers[index];
                Vector2 screenPos = Camera.main.WorldToScreenPoint(mover.Position);
                if (selectionRect.Contains(screenPos))
                {
                    entityManager.SetComponentEnabled<Selected>(entities[index], true);
                    Selected selected = selectedEntities[index];
                    selected.onSelected = true;
                    entityManager.SetComponentData(entities[index], selected);
                }
            }
        }

        private void CreateCameraRay(out EntityManager entityManager, out CollisionWorld collision, out RaycastInput ray)
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery query = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
            var physics = query.GetSingleton<PhysicsWorldSingleton>();
            collision = physics.CollisionWorld;

            UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            ray = new RaycastInput
            {
                Start = cameraRay.GetPoint(0f),
                End = cameraRay.GetPoint(9999f),
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,        //Bitmasks, all bits 1
                    CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                    GroupIndex = 0
                }
            };
        }

        private void SelectSingleUnit()
        {
            CreateCameraRay(out EntityManager entityManager, out CollisionWorld collision, out RaycastInput ray);

            if (collision.CastRay(ray, out Unity.Physics.RaycastHit hit))
            {
                if (entityManager.HasComponent<Selected>(hit.Entity))
                {
                    entityManager.SetComponentEnabled<Selected>(hit.Entity, true);
                    Selected selected = entityManager.GetComponentData<Selected>(hit.Entity);
                    selected.onSelected = true;
                    entityManager.SetComponentData(hit.Entity, selected);
                }
            }
        }

        private bool SelectOverrideTargetZombie()
        {
            CreateCameraRay(out EntityManager entityManager, out CollisionWorld collision, out RaycastInput ray);

            if (collision.CastRay(ray, out Unity.Physics.RaycastHit hit))
            {
                if (entityManager.HasComponent<Faction>(hit.Entity))
                {
                    Faction unit = entityManager.GetComponentData<Faction>(hit.Entity);
                    if(unit.faction == FactionType.Zombie)
                    {
                        EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().WithPresent<TargetOverride>().Build(entityManager);

                        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
                        NativeArray<TargetOverride> targetOverride = query.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                        for (int index = 0; index < targetOverride.Length; ++index)
                        {
                            TargetOverride mover = targetOverride[index];
                            mover.target = hit.Entity;
                            targetOverride[index] = mover;
                            entityManager.SetComponentEnabled<MoveOverride>(entities[index], false);
                        }
                        query.CopyFromComponentDataArray(targetOverride);

                        return true;
                    }
                }
            }
            return false;
        }

        private void MoveSelectedUnitsToPosition(Vector3 pos)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().WithPresent<MoveOverride, TargetOverride>().Build(entityManager);

            NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
            NativeArray<MoveOverride> moveOverride = query.ToComponentDataArray<MoveOverride>(Allocator.Temp);
            NativeArray<TargetOverride> targetOverrides = query.ToComponentDataArray<TargetOverride>(Allocator.Temp);
            NativeArray<float3> movePositions = GenerateMovePositionArray(pos, entities.Length);
            for (int index = 0; index < moveOverride.Length; ++index)
            {
                MoveOverride mover = moveOverride[index];
                mover.targetPos = movePositions[index];
                moveOverride[index] = mover;
                entityManager.SetComponentEnabled<MoveOverride>(entities[index], true);

                TargetOverride targetOverride = targetOverrides[index];
                targetOverride.target = Entity.Null;
                targetOverrides[index] = targetOverride;
            }
            query.CopyFromComponentDataArray(moveOverride);
            query.CopyFromComponentDataArray(targetOverrides);
        }

        private void HandleBarracksRallyPosition(Vector3 pos)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, BuildingBarracks, LocalTransform>().Build(entityManager);

            NativeArray<BuildingBarracks> barracks = query.ToComponentDataArray<BuildingBarracks>(Allocator.Temp);
            NativeArray<LocalTransform> transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            for (int index = 0; index < barracks.Length; ++index)
            {
                BuildingBarracks barrack = barracks[index];
                barrack.rallyPositionOffset = (float3)pos - transforms[index].Position;
                barracks[index] = barrack;
            }
            query.CopyFromComponentDataArray(barracks);
        }

        private NativeArray<float3> GenerateMovePositionArray(float3 targetPos, int positionCount)
        {
            NativeArray<float3> generatedPositions = new NativeArray<float3>(positionCount, Allocator.Temp);
            if (positionCount == 0)
                return generatedPositions;

            generatedPositions[0] = targetPos;
            if (positionCount == 1)
                return generatedPositions;

            int ring = 0;
            int positionIndex = 1;

            while(positionIndex < positionCount)
            {
                int ringPositionCount = minRingPositions + (int)(ring * extraPositionsPerRingMultiplier);

                for(int index = 0; index < ringPositionCount; index++)
                {
                    float angle = index * math.PI2 / ringPositionCount;
                    float3 ringVector = new float3(multiplePositionRingSize * (ring + 1), 0, 0);
                    ringVector = math.rotate(quaternion.RotateY(angle), ringVector);
                    float3 ringPosition = targetPos + ringVector;

                    generatedPositions[positionIndex] = ringPosition;
                    positionIndex++;
                    if (positionIndex >= positionCount)
                        break;
                }
                ring++;
            }
            return generatedPositions;
        }
    }
}