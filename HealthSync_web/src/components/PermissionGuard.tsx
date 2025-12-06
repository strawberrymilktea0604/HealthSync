import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { usePermissions } from '@/hooks/usePermissions';
import { Permission } from '@/types/rbac';
import { useToast } from '@/hooks/use-toast';

interface PermissionGuardProps {
  children: ReactNode;
  permission?: Permission;
  permissions?: Permission[];
  requireAll?: boolean; // true = need all permissions, false = need any permission
  fallback?: ReactNode;
  redirectTo?: string;
  showToast?: boolean;
}

/**
 * Component to guard content based on user permissions
 * Usage:
 * <PermissionGuard permission={Permission.VIEW_USERS}>
 *   <UserList />
 * </PermissionGuard>
 */
export const PermissionGuard = ({
  children,
  permission,
  permissions = [],
  requireAll = true,
  fallback = null,
  redirectTo,
  showToast = true,
}: PermissionGuardProps) => {
  const { hasAllPermissions, hasAnyPermission } = usePermissions();
  const { toast } = useToast();

  // Determine which permissions to check
  const permsToCheck = permission ? [permission] : permissions;

  // Check permissions based on requireAll flag
  const hasAccess = requireAll
    ? hasAllPermissions(permsToCheck)
    : hasAnyPermission(permsToCheck);

  if (!hasAccess) {
    if (showToast) {
      toast({
        title: 'Không có quyền truy cập',
        description: 'Bạn không có quyền thực hiện hành động này',
        variant: 'destructive',
      });
    }

    if (redirectTo) {
      return <Navigate to={redirectTo} replace />;
    }

    return <>{fallback}</>;
  }

  return <>{children}</>;
};

interface ConditionalRenderProps {
  children: ReactNode;
  permission?: Permission;
  permissions?: Permission[];
  requireAll?: boolean;
  fallback?: ReactNode;
}

/**
 * Component to conditionally render content based on permissions
 * Won't show toast or redirect, just hide/show content
 */
export const Can = ({
  children,
  permission,
  permissions = [],
  requireAll = true,
  fallback = null,
}: ConditionalRenderProps) => {
  const { hasAllPermissions, hasAnyPermission } = usePermissions();

  const permsToCheck = permission ? [permission] : permissions;

  const hasAccess = requireAll
    ? hasAllPermissions(permsToCheck)
    : hasAnyPermission(permsToCheck);

  if (!hasAccess) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};
