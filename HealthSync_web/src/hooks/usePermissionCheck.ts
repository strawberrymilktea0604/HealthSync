import { Permission } from '@/types/rbac';
import { usePermissions } from '@/hooks/usePermissions';
import { useToast } from '@/hooks/use-toast';

/**
 * Hook for imperative permission checking in event handlers
 * 
 * Usage:
 * const { checkAndExecute } = usePermissionCheck();
 * 
 * const handleDelete = () => {
 *   checkAndExecute(
 *     Permission.DELETE_USERS,
 *     () => { deleteUser() },
 *     { errorMessage: 'Custom error' }
 *   );
 * };
 */
export const usePermissionCheck = () => {
  const { hasAllPermissions, hasAnyPermission } = usePermissions();
  const { toast } = useToast();

  const checkAndExecute = (
    permission: Permission | Permission[],
    callback: () => void,
    options?: {
      requireAll?: boolean;
      showToast?: boolean;
      errorMessage?: string;
    }
  ) => {
    const {
      requireAll = true,
      showToast = true,
      errorMessage = 'Bạn không có quyền thực hiện hành động này',
    } = options || {};

    const permissions = Array.isArray(permission) ? permission : [permission];
    const hasAccess = requireAll
      ? hasAllPermissions(permissions)
      : hasAnyPermission(permissions);

    if (!hasAccess) {
      if (showToast) {
        toast({
          title: 'Không có quyền truy cập',
          description: errorMessage,
          variant: 'destructive',
        });
      }
      return false;
    }

    callback();
    return true;
  };

  return { checkAndExecute };
};
