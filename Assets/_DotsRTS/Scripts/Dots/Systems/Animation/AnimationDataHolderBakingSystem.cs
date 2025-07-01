using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEditor.PackageManager;

namespace DotsRTS
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    partial struct AnimationDataHolderBakingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Dictionary<AnimationType, int[]> blobAssetData = new Dictionary<AnimationType, int[]>();

            AnimationDataListSO animList = null;
            foreach(var item in SystemAPI.Query<RefRO<AnimationDataholderObjectData>>())
            {
                animList = item.ValueRO.animList.Value;
            }

            foreach (AnimationType type in System.Enum.GetValues(typeof(AnimationType)))
            {
                AnimationDataSO anim = animList.GetAnimationData(type);
                blobAssetData[type] = new int[anim.meshArray.Length];
            }

            foreach (var (animHolder, mesh) in SystemAPI.Query<RefRO<AnimationDataHolderSubEntity>, RefRO<MaterialMeshInfo>>())
            {
                blobAssetData[animHolder.ValueRO.anim][animHolder.ValueRO.meshIndex] = mesh.ValueRO.Mesh;
            }

            foreach(var animDataHolder in SystemAPI.Query<RefRW<AnimationDataHolder>>())
            {
                BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                ref BlobArray<AnimationData> animData = ref builder.ConstructRoot<BlobArray<AnimationData>>();
                BlobBuilderArray<AnimationData> animDataArr = builder.Allocate(ref animData, System.Enum.GetValues(typeof(AnimationType)).Length);

                int animIndex = 0;
                foreach (AnimationType type in System.Enum.GetValues(typeof(AnimationType)))
                {
                    AnimationDataSO anim = animList.GetAnimationData(type);

                    animDataArr[animIndex].frameTimerMax = anim.frameTimerMax;
                    animDataArr[animIndex].frameMax = anim.meshArray.Length;

                    var blobArray = builder.Allocate(ref animDataArr[animIndex].meshes, anim.meshArray.Length);
                    for (int index = 0; index < anim.meshArray.Length; ++index)
                    {
                        blobArray[index] = blobAssetData[type][index];
                    }
                    ++animIndex;
                }


                animDataHolder.ValueRW.animations = builder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);
                builder.Dispose();
            }
        }
    }
}