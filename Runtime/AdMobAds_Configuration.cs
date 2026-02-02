using MirraGames.SDK.Common;
using UnityEngine;

namespace MirraGames.SDK.AdMob
{
    [ProviderConfiguration(typeof(AdMobAds))]
    public class AdMobAds_Configuration : PropertyGroup
    {
        public override string Name => nameof(AdMobAds);

        [field: SerializeField] public string AndroidBannerId { get; private set; } = "ca-app-pub-3940256099942544/6300978111";
        [field: SerializeField] public string IOSBannerId { get; private set; } = "ca-app-pub-3940256099942544/2934735716";
        [field: SerializeField] public string AndroidInterstitialId { get; private set; } = "ca-app-pub-3940256099942544/1033173712";
        [field: SerializeField] public string IOSInterstitialId { get; private set; } = "ca-app-pub-3940256099942544/4411468910";
        [field: SerializeField] public string AndroidRewardedId { get; private set; } = "ca-app-pub-3940256099942544/5224354917";
        [field: SerializeField] public string IOSRewardedId { get; private set; } = "ca-app-pub-3940256099942544/1712485313";

        public override StringProperty[] GetStringProperties()
        {
            return new StringProperty[] {
                new(
                    "Banner ID Android",
                    getter: () => { return AndroidBannerId; },
                    setter: (value) => { AndroidBannerId = value; }
                ),
                new(
                    "Banner ID iOS",
                    getter: () => { return IOSBannerId; },
                    setter: (value) => { IOSBannerId = value; }
                ),
                new(
                    "Interstitial ID Android",
                    getter: () => { return AndroidInterstitialId; },
                    setter: (value) => { AndroidInterstitialId = value; }
                ),
                new(
                    "Interstitial ID iOS",
                    getter: () => { return IOSInterstitialId; },
                    setter: (value) => { IOSInterstitialId = value; }
                ),
                new(
                    "Rewarded ID Android",
                    getter: () => { return AndroidRewardedId; },
                    setter: (value) => { AndroidRewardedId = value; }
                ),
                new(
                    "Rewarded ID iOS",
                    getter: () => { return IOSRewardedId; },
                    setter: (value) => { IOSRewardedId = value; }
                ),
            };
        }

    }
}