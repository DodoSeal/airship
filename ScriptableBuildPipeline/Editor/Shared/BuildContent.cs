using System;
using System.Collections.Generic;
#if UNITY_2019_3_OR_NEWER
using UnityEditor.Build.Content;
#endif
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Utilities;

namespace UnityEditor.Build.Pipeline
{
#if UNITY_2019_3_OR_NEWER
    /// <summary>
    /// Basic implementation of ICustomAssets. Stores the list of Custom Assets generated during the Scriptable Build Pipeline.
    /// <seealso cref="ICustomAssets"/>
    /// </summary>
    [Serializable]
    public class CustomAssets : ICustomAssets
    {
        /// <inheritdoc />
        public List<GUID> Assets { get; private set; }

        /// <summary>
        /// Default constructor, creates an empty CustomAssets.
        /// </summary>
        public CustomAssets()
        {
            Assets = new List<GUID>();
        }
    }
#endif

    /// <summary>
    /// Basic implementation of IBuildContent. Stores the list of Assets to feed the Scriptable Build Pipeline.
    /// <seealso cref="IBuildContent"/>
    /// </summary>
    [Serializable]
    public class BuildContent : IBuildContent
    {
        /// <inheritdoc />
        public List<GUID> Assets { get; private set; }

        /// <inheritdoc />
        public List<GUID> Scenes { get; private set; }

#if UNITY_2019_3_OR_NEWER
        /// <inheritdoc />
        public List<CustomContent> CustomAssets { get; private set; }
#endif

        /// <summary>
        /// Default constructor, creates an empty BuildContent.
        /// </summary>
        public BuildContent() {}

        /// <summary>
        /// Default constructor, takes a set of Assets and converts them to the appropriate properties.
        /// </summary>
        /// <param name="assets">The set of Assets identified by GUID to ensure are packaged with the build</param>
        public BuildContent(IEnumerable<GUID> assets)
        {
            if (assets == null)
                throw new ArgumentNullException("assets");

            Assets = new List<GUID>();
            Scenes = new List<GUID>();
#if UNITY_2019_3_OR_NEWER
            CustomAssets = new List<CustomContent>();
#endif

            foreach (var asset in assets)
            {
                ValidationMethods.Status assetType = ValidationMethods.ValidAsset(asset);
                if (assetType == ValidationMethods.Status.Asset)
                    Assets.Add(asset);
                else if (assetType == ValidationMethods.Status.Scene)
                    Scenes.Add(asset);
                else
                    throw new ArgumentException(string.Format("Asset '{0}' is not a valid Asset or Scene.", asset.ToString()));
            }
        }
    }

    /// <summary>
    /// Basic implementation of IBundleBuildContent. Stores the list of Assets with explicit Asset Bundle layout to feed the Scriptable Build Pipeline.
    /// <seealso cref="IBundleBuildContent"/>
    /// </summary>
    [Serializable]
    public class BundleBuildContent : IBundleBuildContent
    {
        /// <inheritdoc />
        public List<GUID> Assets { get; private set; }

        /// <inheritdoc />
        public List<GUID> Scenes { get; private set; }

#if UNITY_2019_3_OR_NEWER
        /// <inheritdoc />
        public List<CustomContent> CustomAssets { get; private set; }

        /// <inheritdoc />
        public Dictionary<string, List<ResourceFile>> AdditionalFiles { get; private set; }
#endif

        /// <inheritdoc />
        public Dictionary<GUID, string> Addresses { get; private set; }

        /// <inheritdoc />
        public Dictionary<string, List<GUID>> BundleLayout { get; private set; }

        /// <summary>
        /// Default constructor, creates an empty BundleBuildContent.
        /// </summary>
        public BundleBuildContent() {}

        /// <summary>
        /// Default constructor, takes a set of AssetBundleBuild and converts them to the appropriate properties.
        /// </summary>
        /// <param name="bundleBuilds">The set of AssetbundleBuild to be built.</param>
        public BundleBuildContent(IEnumerable<AssetBundleBuild> bundleBuilds)
        {
            if (bundleBuilds == null)
                throw new ArgumentNullException("bundleBuilds");

            Assets = new List<GUID>();
            Scenes = new List<GUID>();
            Addresses = new Dictionary<GUID, string>();
            BundleLayout = new Dictionary<string, List<GUID>>();
#if UNITY_2019_3_OR_NEWER
            CustomAssets = new List<CustomContent>();
            AdditionalFiles = new Dictionary<string, List<ResourceFile>>();
#endif

            foreach (var bundleBuild in bundleBuilds)
            {
                List<GUID> guids;
                BundleLayout.GetOrAdd(bundleBuild.assetBundleName, out guids);
                ValidationMethods.Status bundleType = ValidationMethods.Status.Invalid;

                for (int i = 0; i < bundleBuild.assetNames.Length; i++)
                {
                    string assetPath = bundleBuild.assetNames[i];
                    GUID asset = new GUID(AssetDatabase.AssetPathToGUID(assetPath));

                    // Ensure the path is valid
                    ValidationMethods.Status status = ValidationMethods.ValidAsset(asset);
                    if (status == ValidationMethods.Status.Invalid)
                        throw new ArgumentException(string.Format("Asset '{0}' is not a valid Asset or Scene.", assetPath));

                    // Ensure we do not have a mixed bundle
                    if (bundleType == ValidationMethods.Status.Invalid)
                        bundleType = status;
                    else if (bundleType != status)
                        throw new ArgumentException(string.Format("Asset Bundle '{0}' is invalid because it contains mixed Asset and Scene types.", bundleBuild.assetBundleName));

                    string address = bundleBuild.addressableNames != null && i < bundleBuild.addressableNames.Length && !string.IsNullOrEmpty(bundleBuild.addressableNames[i]) ?
                        bundleBuild.addressableNames[i] : AssetDatabase.GUIDToAssetPath(asset.ToString());

                    // Add the guid to the bundle map
                    guids.Add(asset);
                    // Add the guid & address
                    Addresses.TryAdd(asset, address);

                    // Add the asset to the correct collection
                    if (status == ValidationMethods.Status.Asset)
                        Assets.Add(asset);
                    else if (status == ValidationMethods.Status.Scene)
                        Scenes.Add(asset);
                }
            }
        }
    }
}
