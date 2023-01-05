﻿using System.Collections.Generic;
using ReadyPlayerMe.Core;

namespace ReadyPlayerMe.AvatarLoader
{
    /// <summary>
    ///     This class is responsible for the <see cref="AvatarConfig" />.
    /// </summary>
    public static class AvatarConfigProcessor
    {
        private const string TAG = nameof(AvatarConfigProcessor);

        private const string PARAM_TRUE = "true";
        private const string PARAM_FALSE = "false";

        /// <summary>
        ///     This method converts the <see cref="AvatarConfig" /> data and combines it into a <c>string</c> that can be added to
        ///     an avatar URL.
        /// </summary>
        /// <param name="avatarConfig">Stores the settings of the <see cref="AvatarConfig" /> to use when requesting the avatar.</param>
        /// <returns>The <see cref="AvatarConfig" /> parameters combined as a <c>string</c>.</returns>
        public static string ProcessAvatarConfiguration(AvatarConfig avatarConfig)
        {
            SDKLogger.Log(TAG, "Processing Avatar Configuration");

            return $"?pose={AvatarConfigMap.Pose[avatarConfig.Pose]}" +
                   $"&meshLod={(int) avatarConfig.MeshLod}" +
                   $"&textureAtlas={AvatarConfigMap.TextureAtlas[avatarConfig.TextureAtlas]}" +
                   $"&textureSizeLimit={ProcessTextureSizeLimit(avatarConfig.TextureSizeLimit)}" +
                   $"{ProcessMorphTargets(avatarConfig.MorphTargets)}" +
                   $"&useHands={(avatarConfig.UseHands ? PARAM_TRUE : PARAM_FALSE)}" +
                   $"&useDracoMeshCompression={(avatarConfig.UseDracoCompression ? PARAM_TRUE : PARAM_FALSE)}";
        }

        /// <summary>
        ///     Processes the processes the <paramref name="textureSize" /> and ensures it is a valid value.
        /// </summary>
        /// <param name="textureSize">The value to process.</param>
        /// <returns>A validated <c>int</c>/returns>
        private static int ProcessTextureSizeLimit(int textureSize)
        {
            return textureSize % 2 == 0 ? textureSize : textureSize + 1;
        }

        /// <summary>
        ///     Combines the list of strings in <paramref name="targets" /> into a single valid morph target parameter.
        /// </summary>
        /// <param name="targets">A list of morph targets as strings.</param>
        /// <returns>A query string of combined morph targets.</returns>
        private static string ProcessMorphTargets(List<string> targets)
        {
            if (targets.Count == 0)
            {
                return string.Empty;
            }
            return $"&morphTargets={string.Join(",", targets)}";
        }
    }
}
