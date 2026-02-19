import { create } from "zustand";
import { persist } from "zustand/middleware";

export interface FeatureFlag {
  key: string;
  name: string;
  description: string;
  enabled: boolean;
  enabledForRoles?: string[];
  enabledForUsers?: string[];
  percentage?: number; // For gradual rollout (0-100)
}

interface FeatureFlagsState {
  flags: FeatureFlag[];
  userFeatures: string[]; // Features explicitly enabled for current user
  isFeatureEnabled: (key: string, userRole?: string, userId?: string) => boolean;
  setFlags: (flags: FeatureFlag[]) => void;
  setUserFeatures: (features: string[]) => void;
  toggleFlag: (key: string) => void;
}

// Default feature flags
const defaultFlags: FeatureFlag[] = [
  {
    key: "dark_mode",
    name: "Dark Mode",
    description: "Enable dark mode theme",
    enabled: true,
  },
  {
    key: "skill_tests",
    name: "Skill Tests",
    description: "Enable skill verification tests",
    enabled: true,
  },
  {
    key: "government_tenders",
    name: "Government Tenders",
    description: "Enable government tender bidding",
    enabled: true,
    enabledForRoles: ["admin", "seller", "agency", "company"],
  },
  {
    key: "escrow_payments",
    name: "Escrow Payments",
    description: "Enable escrow payment protection",
    enabled: true,
  },
  {
    key: "video_calls",
    name: "Video Calls",
    description: "Enable in-app video calls",
    enabled: false,
    enabledForRoles: ["admin", "pro_seller"],
  },
  {
    key: "ai_assistant",
    name: "AI Assistant",
    description: "Enable AI-powered assistance",
    enabled: false,
    percentage: 25, // 25% rollout
  },
  {
    key: "advanced_analytics",
    name: "Advanced Analytics",
    description: "Enable advanced analytics dashboard",
    enabled: true,
    enabledForRoles: ["admin", "pro_seller", "company", "agency"],
  },
  {
    key: "multi_language",
    name: "Multi-Language Support",
    description: "Enable multi-language interface",
    enabled: true,
  },
  {
    key: "proctored_tests",
    name: "Proctored Tests",
    description: "Enable proctored skill tests with webcam",
    enabled: false,
  },
  {
    key: "project_management",
    name: "Project Management",
    description: "Enable advanced project management tools",
    enabled: true,
    enabledForRoles: ["admin", "company", "agency"],
  },
  {
    key: "bulk_messaging",
    name: "Bulk Messaging",
    description: "Enable bulk messaging to clients",
    enabled: false,
    enabledForRoles: ["admin", "company", "agency"],
  },
  {
    key: "api_access",
    name: "API Access",
    description: "Enable API access for integrations",
    enabled: false,
    enabledForRoles: ["admin", "enterprise"],
  },
];

export const useFeatureFlagsStore = create<FeatureFlagsState>()(
  persist(
    (set, get) => ({
      flags: defaultFlags,
      userFeatures: [],

      isFeatureEnabled: (key: string, userRole?: string, userId?: string) => {
        const { flags, userFeatures } = get();
        const flag = flags.find((f) => f.key === key);

        if (!flag) return false;
        if (!flag.enabled) return false;

        // Check if explicitly enabled for this user
        if (userId && flag.enabledForUsers?.includes(userId)) {
          return true;
        }

        // Check if enabled in user's personal features
        if (userFeatures.includes(key)) {
          return true;
        }

        // Check if enabled for user's role
        if (userRole && flag.enabledForRoles?.length) {
          if (!flag.enabledForRoles.includes(userRole)) {
            return false;
          }
        }

        // Check percentage rollout
        if (flag.percentage !== undefined && flag.percentage < 100) {
          // Simple deterministic rollout based on userId
          if (userId) {
            const hash = userId.split("").reduce((acc, char) => {
              return acc + char.charCodeAt(0);
            }, 0);
            const userPercentage = hash % 100;
            if (userPercentage >= flag.percentage) {
              return false;
            }
          }
        }

        return true;
      },

      setFlags: (flags) => set({ flags }),

      setUserFeatures: (features) => set({ userFeatures: features }),

      toggleFlag: (key) =>
        set((state) => ({
          flags: state.flags.map((flag) =>
            flag.key === key ? { ...flag, enabled: !flag.enabled } : flag
          ),
        })),
    }),
    {
      name: "feature-flags-storage",
    }
  )
);
