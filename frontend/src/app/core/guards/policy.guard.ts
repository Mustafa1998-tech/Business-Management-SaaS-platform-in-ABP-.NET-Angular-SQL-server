import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService, PermissionService } from '@abp/ng.core';
import { map, take } from 'rxjs/operators';

export const policyGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const permissionService = inject(PermissionService);
  const router = inject(Router);
  const requiredPolicy = route.data?.['requiredPolicy'] as string | undefined;

  if (!requiredPolicy) {
    return true;
  }

  if (!authService.isAuthenticated) {
    return router.createUrlTree(['/account/login'], {
      queryParams: { returnUrl: state.url }
    });
  }

  const loginTree = router.createUrlTree(['/account/login'], {
    queryParams: { returnUrl: state.url }
  });

  return permissionService.getGrantedPolicy$(requiredPolicy).pipe(
    take(1),
    map(isGranted => {
      if (isGranted) {
        return true;
      }

      if (requiredPolicy === 'SaasSystem.Dashboard' || state.url.startsWith('/dashboard')) {
        return loginTree;
      }

      return router.createUrlTree(['/dashboard']);
    })
  );
};
