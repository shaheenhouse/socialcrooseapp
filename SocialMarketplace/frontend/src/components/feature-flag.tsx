"use client";

import { ReactNode } from "react";
import { useFeatureFlagsStore } from "@/store/feature-flags-store";
import { useAuthStore } from "@/store/auth-store";

interface FeatureFlagProps {
  flag: string;
  children: ReactNode;
  fallback?: ReactNode;
}

/**
 * Feature Flag Component
 * Conditionally renders children based on feature flag status
 * 
 * @example
 * <FeatureFlag flag="video_calls">
 *   <VideoCallButton />
 * </FeatureFlag>
 * 
 * @example
 * <FeatureFlag flag="ai_assistant" fallback={<ComingSoonBadge />}>
 *   <AIAssistantPanel />
 * </FeatureFlag>
 */
export function FeatureFlag({ flag, children, fallback = null }: FeatureFlagProps) {
  const { isFeatureEnabled } = useFeatureFlagsStore();
  const { user } = useAuthStore();

  const enabled = isFeatureEnabled(flag, user?.roles?.[0], user?.id);

  if (!enabled) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}

/**
 * Hook to check if a feature is enabled
 * 
 * @example
 * const isVideoCallsEnabled = useFeatureFlag("video_calls");
 */
export function useFeatureFlag(flag: string): boolean {
  const { isFeatureEnabled } = useFeatureFlagsStore();
  const { user } = useAuthStore();

  return isFeatureEnabled(flag, user?.roles?.[0], user?.id);
}

/**
 * Higher-order component for feature flags
 * 
 * @example
 * const ProtectedComponent = withFeatureFlag(MyComponent, "advanced_feature");
 */
export function withFeatureFlag<P extends object>(
  Component: React.ComponentType<P>,
  flag: string,
  FallbackComponent?: React.ComponentType
) {
  return function WrappedComponent(props: P) {
    const isEnabled = useFeatureFlag(flag);

    if (!isEnabled) {
      return FallbackComponent ? <FallbackComponent /> : null;
    }

    return <Component {...props} />;
  };
}
