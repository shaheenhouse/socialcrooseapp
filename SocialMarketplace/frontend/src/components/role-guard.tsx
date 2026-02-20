"use client";

import { ReactNode, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/store/auth-store";

export type UserRole =
  | "user"
  | "seller"
  | "buyer"
  | "admin"
  | "moderator"
  | "company"
  | "agency"
  | "employee"
  | "pro_seller"
  | "enterprise";

interface RoleGuardProps {
  children: ReactNode;
  allowedRoles: UserRole[];
  fallback?: ReactNode;
  redirectTo?: string;
}

/**
 * Role Guard Component
 * Protects content based on user roles
 * 
 * @example
 * <RoleGuard allowedRoles={["admin", "moderator"]}>
 *   <AdminPanel />
 * </RoleGuard>
 * 
 * @example
 * <RoleGuard 
 *   allowedRoles={["seller", "pro_seller"]} 
 *   fallback={<UpgradePrompt />}
 * >
 *   <SellerDashboard />
 * </RoleGuard>
 */
export function RoleGuard({
  children,
  allowedRoles,
  fallback = null,
  redirectTo,
}: RoleGuardProps) {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();

  const hasAccess =
    isAuthenticated &&
    user?.roles?.length &&
    user.roles.some((r) => allowedRoles.includes(r as UserRole));

  useEffect(() => {
    if (!hasAccess && redirectTo) {
      router.push(redirectTo);
    }
  }, [hasAccess, redirectTo, router]);

  if (!hasAccess) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}

/**
 * Hook to check if user has a specific role
 * 
 * @example
 * const isAdmin = useRole("admin");
 * const isSeller = useRole(["seller", "pro_seller"]);
 */
export function useRole(role: UserRole | UserRole[]): boolean {
  const { user, isAuthenticated } = useAuthStore();

  if (!isAuthenticated || !user?.roles?.length) {
    return false;
  }

  const allowedRoles = Array.isArray(role) ? role : [role];
  return user.roles.some((r) => allowedRoles.includes(r as UserRole));
}

/**
 * Hook to check if user has any of the specified roles
 * 
 * @example
 * const canManageUsers = useHasPermission(["admin", "moderator"]);
 */
export function useHasPermission(allowedRoles: UserRole[]): boolean {
  return useRole(allowedRoles);
}

/**
 * Higher-order component for role-based access
 * 
 * @example
 * const AdminOnlyComponent = withRoleGuard(MyComponent, ["admin"]);
 */
export function withRoleGuard<P extends object>(
  Component: React.ComponentType<P>,
  allowedRoles: UserRole[],
  FallbackComponent?: React.ComponentType
) {
  return function WrappedComponent(props: P) {
    const hasAccess = useRole(allowedRoles);

    if (!hasAccess) {
      return FallbackComponent ? <FallbackComponent /> : null;
    }

    return <Component {...props} />;
  };
}

/**
 * Permission definitions for common actions
 */
export const Permissions = {
  // Admin permissions
  MANAGE_USERS: ["admin"] as UserRole[],
  MANAGE_CONTENT: ["admin", "moderator"] as UserRole[],
  VIEW_ANALYTICS: ["admin", "moderator"] as UserRole[],
  MANAGE_SETTINGS: ["admin"] as UserRole[],

  // Seller permissions
  CREATE_SERVICE: ["seller", "pro_seller", "company", "agency"] as UserRole[],
  MANAGE_STORE: ["seller", "pro_seller", "company", "agency"] as UserRole[],
  VIEW_ORDERS: ["seller", "pro_seller", "buyer", "company", "agency"] as UserRole[],

  // Company/Agency permissions
  MANAGE_TEAM: ["company", "agency"] as UserRole[],
  VIEW_TEAM_ANALYTICS: ["company", "agency"] as UserRole[],
  BID_ON_TENDERS: ["seller", "pro_seller", "company", "agency"] as UserRole[],

  // Pro features
  BULK_MESSAGING: ["pro_seller", "company", "agency"] as UserRole[],
  ADVANCED_ANALYTICS: ["pro_seller", "company", "agency", "enterprise"] as UserRole[],
  API_ACCESS: ["enterprise", "admin"] as UserRole[],
};
