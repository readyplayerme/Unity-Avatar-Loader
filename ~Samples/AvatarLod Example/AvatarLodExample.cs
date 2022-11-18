﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ReadyPlayerMe.AvatarLoader;
using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe
{
    public class AvatarLodExample : MonoBehaviour
    {
        private const string LOD_MESH_SUFFIX = "_LOD";
        private const float CULL_TRANSITION = 0.05f;
        
        [SerializeField] private AvatarLodExampleUI lodExampleUI;
        [SerializeField] private string avatarUrl;
        [SerializeField] private AvatarConfig[] lodConfigs;

        private LODGroup lodGroup;
        private bool loading;
        private GameObject mainAvatar;
        private SkinnedMeshRenderer mainMeshRenderer;
        private readonly List<SkinnedMeshRenderer> meshRenderersList = new List<SkinnedMeshRenderer>();

        private void Start()
        {
            ApplicationData.Log();
            if (lodExampleUI) lodExampleUI.Init();
            LoadLodAvatar();
        }

        private async void LoadLodAvatar()
        {
            var bodyType = BodyType.None;

            foreach (var config in lodConfigs)
            {
                var lodLevel = (int) config.MeshLod;

                AvatarObjectLoader loader = new AvatarObjectLoader();
                loader.AvatarConfig = config;
                loader.LoadAvatar(avatarUrl);
                loader.OnCompleted += (sender, args) =>
                {
                    if (mainAvatar == null)
                    {
                        bodyType = args.Metadata.BodyType;
                        mainAvatar = Instantiate(args.Avatar);
                        mainAvatar.name = args.Avatar.name + LOD_MESH_SUFFIX;

                        mainMeshRenderer = mainAvatar.GetComponentInChildren<SkinnedMeshRenderer>();
                        var meshTransform = mainMeshRenderer.transform;
                        meshTransform.name = meshTransform.name += $"{LOD_MESH_SUFFIX}{lodLevel}";
                        mainMeshRenderer.enabled = false;
                        meshRenderersList.Add(mainMeshRenderer);
                    }
                    else
                    {
                        var lodSkinnedMeshRenderer = args.Avatar.GetComponentInChildren<SkinnedMeshRenderer>();
                        lodSkinnedMeshRenderer.rootBone = mainMeshRenderer.rootBone;
                        lodSkinnedMeshRenderer.bones = mainMeshRenderer.bones;
                        lodSkinnedMeshRenderer.transform.name += $"{LOD_MESH_SUFFIX}{lodLevel}";
                        lodSkinnedMeshRenderer.transform.SetParent(mainAvatar.transform);
                        lodSkinnedMeshRenderer.transform.SetSiblingIndex(lodLevel);
                        lodSkinnedMeshRenderer.enabled = false;
                        meshRenderersList.Add(lodSkinnedMeshRenderer);
                    }

                    Destroy(args.Avatar);

                    loading = false;
                };
                loading = true;

                while (loading)
                {
                    await Task.Yield();
                }
            }

            AddLodGroup();
            AvatarAnimatorHelper.SetupAnimator(bodyType, mainAvatar);
            if (lodExampleUI) lodExampleUI.Show();
        }

        private void AddLodGroup()
        {
            lodGroup = mainAvatar.AddComponent<LODGroup>();
            var lods = new LOD[lodConfigs.Length];
            for (var i = 0; i < lodConfigs.Length; i++)
            {
                meshRenderersList[i].enabled = true;
                lods[i] = new LOD(CalculateTransitionHeight(i), new Renderer[] { meshRenderersList[i] });
            }
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();

            lodExampleUI.LodGroup = lodGroup;
        }

        private float CalculateTransitionHeight(int lodConfig)
        {
            var lodTransition = (lodConfig + 1f) / lodConfigs.Length;
            return (1 - lodTransition + CULL_TRANSITION);
        }

        private void OnDestroy()
        {
            if (mainAvatar != null)
            {
                Destroy(mainAvatar);
            }
        }
    }
}

