import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { BankingService } from '../services/banking';

export const authGuard: CanActivateFn = (route, state) => {
  const bankingService = inject(BankingService);
  const router = inject(Router);

  if (bankingService.isLoggedIn()) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};