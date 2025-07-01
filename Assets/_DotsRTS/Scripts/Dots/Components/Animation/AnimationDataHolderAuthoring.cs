using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;

namespace DotsRTS
{
    public struct AnimationData
    {
        public float frameTimerMax;
        public int frameMax;
        public BlobArray<BatchMeshID> meshes;
    }

    public struct AnimationDataHolder: IComponentData
    {
        public BlobAssetReference<BlobArray<AnimationData>> animations;
    }

    class AnimationDataHolderAuthoring : MonoBehaviour
    {
        public AnimationDataListSO animations;

        class AnimationDataHolderAuthoringBaker : Baker<AnimationDataHolderAuthoring>
        {
            public override void Bake(AnimationDataHolderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AnimationDataHolder holder = new AnimationDataHolder();
                CreateBlob(ref holder, authoring);
                AddComponent(entity, holder);
            }

            private void CreateBlob(ref AnimationDataHolder holder, AnimationDataHolderAuthoring authoring)
            {
                BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                ref BlobArray<AnimationData> animData = ref builder.ConstructRoot<BlobArray<AnimationData>>();
                BlobBuilderArray<AnimationData> animDataArr = builder.Allocate(ref animData, System.Enum.GetValues(typeof(AnimationType)).Length);

                EntitiesGraphicsSystem graphics = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

                int animIndex = 0;
                foreach (AnimationType type in System.Enum.GetValues(typeof(AnimationType)))
                {
                    AnimationDataSO anim = authoring.animations.GetAnimationData(type);

                    animDataArr[animIndex].frameTimerMax = anim.frameTimerMax;
                    animDataArr[animIndex].frameMax = anim.meshArray.Length;

                    var blobArray = builder.Allocate(ref animDataArr[animIndex].meshes, anim.meshArray.Length);
                    for (int index = 0; index < anim.meshArray.Length; ++index)
                    {
                        blobArray[index] = graphics.RegisterMesh(anim.meshArray[index]);
                    }
                    ++animIndex;
                }


                holder.animations = builder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);

                builder.Dispose();
                AddBlobAsset(ref holder.animations, out Unity.Entities.Hash128 hash);
            }
        }
    }
}