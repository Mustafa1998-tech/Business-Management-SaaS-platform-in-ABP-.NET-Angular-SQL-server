import { ChangeDetectionStrategy, Component } from '@angular/core';

interface UserRow {
  id: string;
  name: string;
  email: string;
  status: 'Active' | 'Blocked';
}

@Component({
  selector: 'app-users-page',
  templateUrl: './users-page.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersPageComponent {
  readonly users: UserRow[] = [
    { id: 'USR-1001', name: 'Nora Smith', email: 'nora@tenant-one.com', status: 'Active' },
    { id: 'USR-1002', name: 'Ryan Lee', email: 'ryan@tenant-one.com', status: 'Blocked' },
    { id: 'USR-1003', name: 'Amir Khan', email: 'amir@tenant-two.com', status: 'Active' }
  ];

  trackByUser(_: number, item: UserRow): string {
    return item.id;
  }
}
