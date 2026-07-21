import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { BankingService, Account, Transaction } from '../../services/banking';

@Component({
  selector: 'app-dashboard',
  imports: [FormsModule, CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  accounts = signal<Account[]>([]);
  transactions = signal<Transaction[]>([]);

  fromAccountId = 0;
  toAccountId = 0;
  amount = 0;
  errorMessage = '';

  constructor(private bankingService: BankingService, private router: Router) {}

  ngOnInit() {
    this.loadAccounts();
    this.loadTransactions();
  }

  loadAccounts() {
    this.bankingService.getAccounts().subscribe({
      next: (data) => this.accounts.set(data),
      error: (err) => console.error('Failed to load accounts', err)
    });
  }

  loadTransactions() {
    this.bankingService.getTransactions().subscribe({
      next: (data) => this.transactions.set(data),
      error: (err) => console.error('Failed to load transactions', err)
    });
  }

  makeTransfer() {
    this.errorMessage = '';
    this.bankingService.createTransaction({
      fromAccountId: this.fromAccountId,
      toAccountId: this.toAccountId,
      amount: this.amount
    }).subscribe({
      next: () => {
        this.loadTransactions();
        this.fromAccountId = 0;
        this.toAccountId = 0;
        this.amount = 0;
      },
      error: (err) => {
        this.errorMessage = 'Transfer failed. Check console for details.';
        console.error('Transfer failed', err);
      }
    });
  }

  logout() {
    this.bankingService.logout();
    this.router.navigate(['/login']);
  }
}