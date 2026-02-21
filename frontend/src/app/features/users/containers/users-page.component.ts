import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

interface UserRow {
  id: string;
  name: string;
  email: string;
  role: string;
  status: 'Active' | 'Blocked';
}

@Component({
  selector: 'app-users-page',
  templateUrl: './users-page.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersPageComponent {
  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(128)]],
    email: ['', [Validators.required, Validators.email]],
    role: ['User', Validators.required],
    status: ['Active' as UserRow['status'], Validators.required]
  });

  users: UserRow[] = [
    { id: 'USR-1001', name: 'Nora Smith', email: 'nora@tenant-one.com', role: 'Manager', status: 'Active' },
    { id: 'USR-1002', name: 'Ryan Lee', email: 'ryan@tenant-one.com', role: 'Analyst', status: 'Blocked' },
    { id: 'USR-1003', name: 'Amir Khan', email: 'amir@tenant-two.com', role: 'Admin', status: 'Active' }
  ];

  filter = '';
  modalVisible = false;
  editingUserId?: string;

  constructor(private readonly fb: FormBuilder) {}

  get visibleUsers(): UserRow[] {
    const keyword = this.filter.trim().toLowerCase();
    if (!keyword) {
      return this.users;
    }

    return this.users.filter(user =>
      user.name.toLowerCase().includes(keyword) ||
      user.email.toLowerCase().includes(keyword) ||
      user.role.toLowerCase().includes(keyword));
  }

  openCreateModal(): void {
    this.editingUserId = undefined;
    this.form.reset({
      name: '',
      email: '',
      role: 'User',
      status: 'Active'
    });
    this.modalVisible = true;
  }

  openEditModal(user: UserRow): void {
    this.editingUserId = user.id;
    this.form.reset({
      name: user.name,
      email: user.email,
      role: user.role,
      status: user.status
    });
    this.modalVisible = true;
  }

  closeModal(): void {
    this.modalVisible = false;
    this.editingUserId = undefined;
  }

  saveUser(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    if (this.editingUserId) {
      this.users = this.users.map(user =>
        user.id === this.editingUserId
          ? {
              ...user,
              name: value.name.trim(),
              email: value.email.trim(),
              role: value.role,
              status: value.status
            }
          : user);
    } else {
      const nextId = `USR-${Date.now()}`;
      this.users = [
        {
          id: nextId,
          name: value.name.trim(),
          email: value.email.trim(),
          role: value.role,
          status: value.status
        },
        ...this.users
      ];
    }

    this.closeModal();
  }

  setBlocked(user: UserRow, isBlocked: boolean): void {
    this.users = this.users.map(item =>
      item.id === user.id
        ? { ...item, status: isBlocked ? 'Blocked' : 'Active' }
        : item);
  }

  deleteUser(user: UserRow): void {
    if (!confirm(`Delete user "${user.name}"?`)) {
      return;
    }

    this.users = this.users.filter(item => item.id !== user.id);
  }

  trackByUser(_: number, item: UserRow): string {
    return item.id;
  }
}
