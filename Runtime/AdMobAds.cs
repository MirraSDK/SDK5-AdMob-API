using GoogleMobileAds.Api;
using MirraGames.SDK.Common;
using System;
using UnityEngine;
using Logger = MirraGames.SDK.Common.Logger;

namespace MirraGames.SDK.AdMob
{
    [Provider(typeof(IAds))]
    public class AdMobAds : CommonAds
    {
        private readonly AdMobAds_Configuration Configuration;

        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;

        private Action OnInterstitialOpen;
        private Action<bool> OnInterstitialClose;

        private Action OnRewardedOpen;
        private Action<bool> OnRewardedClose;
        private bool IsRewardedSuccess = false;

        public AdMobAds(AdMobAds_Configuration configuration, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            Configuration = configuration;
            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(OnMobileAdsInitialized);
        }

        protected override void InvokeBannerImpl()
        {
            // If we already have a banner, destroy the old one.
            DisableBannerImpl();
            // Create a 320x50 banner at top of the screen.
            bannerView = new BannerView(GetBannerId(), AdSize.Banner, AdPosition.Bottom);
            // Banner callbacks.
            bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
            bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
            bannerView.OnAdPaid += OnBannerAdPaid;
            bannerView.OnAdImpressionRecorded += OnBannerAdImpressionRecorded;
            bannerView.OnAdClicked += OnBannerAdClicked;
            bannerView.OnAdFullScreenContentOpened += OnBannerAdFullScreenContentOpened;
            bannerView.OnAdFullScreenContentClosed += OnBannerAdFullScreenContentClosed;
            // Create our request used to load the ad.
            AdRequest adRequest = new();
            // Load the banner with the request.
            Logger.CreateText(this, "Loading banner ad.");
            bannerView.LoadAd(adRequest);
        }

        protected override void DisableBannerImpl()
        {
            if (bannerView != null)
            {
                Logger.CreateText(this, "Destroying banner view.");
                bannerView.Destroy();
                bannerView = null;
            }
        }

        protected override void RefreshBannerImpl()
        {
            Logger.NotImplementedWarning(this, nameof(RefreshBannerImpl));
        }

        protected override void InvokeInterstitialImpl(InterstitialParameters parameters, Action onOpen, Action<bool> onClose)
        {
            OnInterstitialOpen = onOpen;
            OnInterstitialClose = onClose;
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                Logger.CreateText(this, "Showing interstitial ad.");
                interstitialAd.Show();
            }
            else
            {
                Logger.CreateText(this, "Interstitial ad is not ready yet.");
            }
        }

        protected override void InvokeRewardedImpl(RewardedParameters parameters, Action onOpen, Action<bool> onClose)
        {
            OnRewardedOpen = onOpen;
            OnRewardedClose = onClose;
            IsRewardedSuccess = false;
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                Logger.CreateText(this, "Showing rewarded ad.");
                rewardedAd.Show((Reward reward) =>
                {
                    IsRewardedSuccess = true;
                });
            }
            else
            {
                Logger.CreateText(this, "Rewarded ad is not ready yet.");
            }
        }

        /// <summary>
        /// This callback is called once the MobileAds SDK is initialized.
        /// </summary>
        private void OnMobileAdsInitialized(InitializationStatus initStatus)
        {
            LoadInterstitialAd();
            LoadRewardedAd();
        }

        /// <summary>
        /// Loads the interstitial ad.
        /// </summary>
        private void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }
            Logger.CreateText(this, "Loading the interstitial ad.");
            // Create our request used to load the ad.
            AdRequest adRequest = new();
            // Send the request to load the ad.
            InterstitialAd.Load(GetInterstitialId(), adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                // If error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Logger.CreateError(this, $"Interstitial ad failed to load an ad with error: {error}.");
                    return;
                }
                Logger.CreateText(this, $"Interstitial ad loaded with response : {ad.GetResponseInfo()}.");
                interstitialAd = ad;
                // Interstitial callbacks.
                interstitialAd.OnAdPaid += OnInterstitialAdPaid;
                interstitialAd.OnAdImpressionRecorded += OnInterstitialAdImpressionRecorded;
                interstitialAd.OnAdClicked += OnInterstitialAdClicked;
                interstitialAd.OnAdFullScreenContentOpened += OnInterstitialAdFullScreenContentOpened;
                interstitialAd.OnAdFullScreenContentClosed += OnInterstitialAdFullScreenContentClosed;
                interstitialAd.OnAdFullScreenContentFailed += OnInterstitialAdFullScreenContentFailed;
            });
        }

        /// <summary>
        /// Loads the rewarded ad.
        /// </summary>
        private void LoadRewardedAd()
        {
            // Clean up the old ad before loading a new one.
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }
            Logger.CreateText(this, "Loading the rewarded ad.");
            // Create our request used to load the ad.
            AdRequest adRequest = new();
            // Send the request to load the ad.
            RewardedAd.Load(GetRewardedId(), adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                // If error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Logger.CreateError(this, $"Rewarded ad failed to load an ad with error: {error}.");
                    return;
                }
                Logger.CreateText(this, $"Rewarded ad loaded with response : {ad.GetResponseInfo()}.");
                rewardedAd = ad;
                // Rewarded callbacks.
                rewardedAd.OnAdPaid += OnRewardedAdPaid;
                rewardedAd.OnAdImpressionRecorded += OnRewardedAdImpressionRecorded;
                rewardedAd.OnAdClicked += OnRewardedAdClicked;
                rewardedAd.OnAdFullScreenContentOpened += OnRewardedAdFullScreenContentOpened;
                rewardedAd.OnAdFullScreenContentClosed += OnRewardedAdFullScreenContentClosed;
                rewardedAd.OnAdFullScreenContentFailed += OnRewardedAdFullScreenContentFailed;
            });
        }

        private string GetBannerId()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Configuration.AndroidBannerId;
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Configuration.IOSBannerId;
            }
            return string.Empty;
        }

        private string GetInterstitialId()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Configuration.AndroidInterstitialId;
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Configuration.IOSInterstitialId;
            }
            return string.Empty;
        }

        private string GetRewardedId()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Configuration.AndroidRewardedId;
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Configuration.IOSRewardedId;
            }
            return string.Empty;
        }

        // Banner callbacks.

        /// <summary>
        /// Raised when an ad is loaded into the banner view.
        /// </summary>
        private void OnBannerAdLoaded()
        {
            Logger.CreateText(this, $"Banner view loaded an ad with response: {bannerView.GetResponseInfo()}.");
        }

        /// <summary>
        /// Raised when an ad fails to load into the banner view.
        /// </summary>
        private void OnBannerAdLoadFailed(LoadAdError error)
        {
            Logger.CreateError(this, $"Banner view failed to load an ad with error: {error}.");
        }

        /// <summary>
        /// Raised when the ad is estimated to have earned money.
        /// </summary>
        private void OnBannerAdPaid(AdValue adValue)
        {
            Logger.CreateText(this, $"Banner view paid {adValue.Value} {adValue.CurrencyCode}.");
        }

        /// <summary>
        /// Raised when an impression is recorded for an ad.
        /// </summary>
        private void OnBannerAdImpressionRecorded()
        {
            Logger.CreateText(this, "Banner view recorded an impression.");
        }

        /// <summary>
        /// Raised when a click is recorded for an ad.
        /// </summary>
        private void OnBannerAdClicked()
        {
            Logger.CreateText(this, "Banner view was clicked.");
        }

        /// <summary>
        /// Raised when an ad opened full screen content.
        /// </summary>
        private void OnBannerAdFullScreenContentOpened()
        {
            Logger.CreateText(this, "Banner view full screen content opened.");
            IsBannerVisible = true;
        }

        /// <summary>
        /// Raised when the ad closed full screen content.
        /// </summary>
        private void OnBannerAdFullScreenContentClosed()
        {
            Logger.CreateText(this, "Banner view full screen content closed.");
            IsBannerVisible = false;
        }

        // Interstitial callbacks.

        /// <summary>
        /// Raised when the ad is estimated to have earned money.
        /// </summary>
        private void OnInterstitialAdPaid(AdValue adValue)
        {
            Logger.CreateText(this, $"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}.");
        }

        /// <summary>
        /// Raised when an impression is recorded for an ad.
        /// </summary>
        private void OnInterstitialAdImpressionRecorded()
        {
            Logger.CreateText(this, "Interstitial ad recorded an impression.");
        }

        /// <summary>
        /// Raised when a click is recorded for an ad.
        /// </summary>
        private void OnInterstitialAdClicked()
        {
            Logger.CreateText(this, "Interstitial ad was clicked.");
        }

        /// <summary>
        /// Raised when an ad opened full screen content.
        /// </summary>
        private void OnInterstitialAdFullScreenContentOpened()
        {
            Logger.CreateText(this, "Interstitial ad full screen content opened.");
            OnInterstitialOpen?.Invoke();
            IsInterstitialVisible = true;
        }

        /// <summary>
        /// Raised when the ad closed full screen content.
        /// </summary>
        private void OnInterstitialAdFullScreenContentClosed()
        {
            Logger.CreateText(this, "Interstitial ad full screen content closed.");
            OnInterstitialClose?.Invoke(true);
            IsInterstitialVisible = false;
            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        }

        /// <summary>
        /// Raised when the ad failed to open full screen content.
        /// </summary>
        private void OnInterstitialAdFullScreenContentFailed(AdError error)
        {
            Logger.CreateError(this, $"Interstitial ad failed to open full screen content: {error}.");
            OnInterstitialClose?.Invoke(false);
            IsInterstitialVisible = false;
            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        }

        // Rewarded callbacks.

        /// <summary>
        /// Raised when the ad is estimated to have earned money.
        /// </summary>
        private void OnRewardedAdPaid(AdValue adValue)
        {
            Logger.CreateText(this, $"Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}.");
        }

        /// <summary>
        /// Raised when an impression is recorded for an ad.
        /// </summary>
        private void OnRewardedAdImpressionRecorded()
        {
            Logger.CreateText(this, "Rewarded ad recorded an impression.");
        }

        /// <summary>
        /// Raised when a click is recorded for an ad.
        /// </summary>
        private void OnRewardedAdClicked()
        {
            Logger.CreateText(this, "Rewarded ad was clicked.");
        }

        /// <summary>
        /// Raised when an ad opened full screen content.
        /// </summary>
        private void OnRewardedAdFullScreenContentOpened()
        {
            Logger.CreateText(this, "Rewarded ad full screen content opened.");
            OnRewardedOpen?.Invoke();
            IsRewardedVisible = true;
        }

        /// <summary>
        /// Raised when the ad closed full screen content.
        /// </summary>
        private void OnRewardedAdFullScreenContentClosed()
        {
            Logger.CreateText(this, "Rewarded ad full screen content closed.");
            OnRewardedClose?.Invoke(IsRewardedSuccess);
            IsRewardedVisible = false;
            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        }

        /// <summary>
        /// Raised when the ad failed to open full screen content.
        /// </summary>
        private void OnRewardedAdFullScreenContentFailed(AdError error)
        {
            Logger.CreateError(this, $"Rewarded ad failed to open full screen content: {error}.");
            OnRewardedClose?.Invoke(false);
            IsRewardedVisible = false;
            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        }
    }
}