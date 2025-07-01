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
        public BlobArray<int> meshes;
    }

    public struct AnimationDataHolder: IComponentData
    {
        public BlobAssetReference<BlobArray<AnimationData>> animations;
    }

    public struct AnimationDataHolderSubEntity: IComponentData
    {
        public AnimationType anim;
        public int meshIndex;
    }

    public struct AnimationDataholderObjectData: IComponentData
    {
        public UnityObjectRef<AnimationDataListSO> animList;
    }

    class AnimationDataHolderAuthoring : MonoBehaviour
    {
        public AnimationDataListSO animations;
        public Material defaultMat;

        class AnimationDataHolderAuthoringBaker : Baker<AnimationDataHolderAuthoring>
        {
            public override void Bake(AnimationDataHolderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AnimationDataHolder holder = new AnimationDataHolder();
                CreateBlob(ref holder, authoring);

                AddComponent(entity, new AnimationDataholderObjectData
                {
                    animList = authoring.animations
                });
                AddComponent(entity, holder);
            }

            private void CreateBlob(ref AnimationDataHolder holder, AnimationDataHolderAuthoring authoring)
            {
                int animIndex = 0;
                foreach (AnimationType type in System.Enum.GetValues(typeof(AnimationType)))
                {
                    AnimationDataSO anim = authoring.animations.GetAnimationData(type);

                    for (int index = 0; index < anim.meshArray.Length; ++index)
                    {
                        Entity additionalEntity = CreateAdditionalEntity(TransformUsageFlags.None, true, "SubEntity");
                        AddComponent(additionalEntity, new MaterialMeshInfo());
                        AddComponent(additionalEntity, new RenderMeshUnmanaged
                        {
                            mesh = anim.meshArray[index],
                            materialForSubMesh = authoring.defaultMat
                        });
                        AddComponent(additionalEntity, new AnimationDataHolderSubEntity
                        {
                            anim = type,
                            meshIndex = index
                        });
                    }
                    ++animIndex;
                }
            }
        }
    }
}