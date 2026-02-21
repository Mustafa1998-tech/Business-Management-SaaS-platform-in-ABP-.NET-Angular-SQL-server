import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { CreateUpdateProjectDto, ProjectDto } from '../../../proxy/models';
import { ProjectService } from '../../../proxy/projects/project.service';

interface ProjectStatusOption {
  readonly value: number;
  readonly label: string;
}

@Component({
  selector: 'app-projects-page',
  templateUrl: './projects-page.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectsPageComponent implements OnInit {
  readonly statusOptions: ProjectStatusOption[] = [
    { value: 1, label: 'Planned' },
    { value: 2, label: 'Active' },
    { value: 3, label: 'On Hold' },
    { value: 4, label: 'Completed' },
    { value: 5, label: 'Cancelled' }
  ];

  readonly form = this.fb.nonNullable.group({
    customerId: ['', Validators.required],
    name: ['', [Validators.required, Validators.maxLength(128)]],
    description: ['', [Validators.required, Validators.maxLength(512)]],
    startDate: ['', Validators.required],
    endDate: [''],
    budget: [0, [Validators.required, Validators.min(0)]],
    status: [1, Validators.required]
  });

  projects: ProjectDto[] = [];
  totalCount = 0;
  pageIndex = 0;
  readonly pageSize = 15;

  filter = '';
  statusFilter?: number;

  isLoading = true;
  hasError = false;
  isSaving = false;
  modalVisible = false;
  editingProjectId?: string;

  constructor(
    private readonly projectService: ProjectService,
    private readonly fb: FormBuilder,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.hasError = false;

    this.projectService
      .getList({
        skipCount: this.pageIndex * this.pageSize,
        maxResultCount: this.pageSize,
        sorting: 'startDate desc',
        filter: this.filter.trim(),
        status: this.statusFilter
      })
      .subscribe({
        next: result => {
          this.projects = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.hasError = true;
          this.isLoading = false;
          this.cdr.markForCheck();
        }
      });
  }

  openCreateModal(): void {
    this.editingProjectId = undefined;
    this.form.reset({
      customerId: '',
      name: '',
      description: '',
      startDate: this.todayDate(),
      endDate: '',
      budget: 0,
      status: 1
    });
    this.modalVisible = true;
    this.cdr.markForCheck();
  }

  openEditModal(project: ProjectDto): void {
    this.editingProjectId = project.id;
    this.form.reset({
      customerId: project.customerId,
      name: project.name,
      description: project.description,
      startDate: project.startDate.slice(0, 10),
      endDate: project.endDate?.slice(0, 10) ?? '',
      budget: project.budget,
      status: project.status
    });
    this.modalVisible = true;
    this.cdr.markForCheck();
  }

  closeModal(): void {
    this.modalVisible = false;
    this.isSaving = false;
    this.cdr.markForCheck();
  }

  save(): void {
    if (this.form.invalid || this.isSaving) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload: CreateUpdateProjectDto = {
      customerId: value.customerId,
      name: value.name.trim(),
      description: value.description.trim(),
      startDate: value.startDate,
      endDate: value.endDate || undefined,
      budget: Number(value.budget),
      status: Number(value.status)
    };

    this.isSaving = true;

    const request$ = this.editingProjectId
      ? this.projectService.update(this.editingProjectId, payload)
      : this.projectService.create(payload);

    request$.subscribe({
      next: () => {
        this.closeModal();
        this.load();
      },
      error: () => {
        this.isSaving = false;
        this.cdr.markForCheck();
      }
    });
  }

  quickToggleHold(project: ProjectDto): void {
    const targetStatus = project.status === 2 ? 3 : 2;
    const payload: CreateUpdateProjectDto = {
      customerId: project.customerId,
      name: project.name,
      description: project.description,
      startDate: project.startDate.slice(0, 10),
      endDate: project.endDate?.slice(0, 10),
      budget: project.budget,
      status: targetStatus
    };

    this.projectService.update(project.id, payload).subscribe({
      next: () => this.load(),
      error: () => {
        this.hasError = true;
        this.cdr.markForCheck();
      }
    });
  }

  delete(project: ProjectDto): void {
    if (!confirm(`Delete project "${project.name}"?`)) {
      return;
    }

    this.projectService.delete(project.id).subscribe({
      next: () => this.load(),
      error: () => {
        this.hasError = true;
        this.cdr.markForCheck();
      }
    });
  }

  onPageChanged(direction: 1 | -1): void {
    const nextIndex = this.pageIndex + direction;
    if (nextIndex < 0 || nextIndex > this.maxPageIndex) {
      return;
    }

    this.pageIndex = nextIndex;
    this.load();
  }

  statusLabel(status: number): string {
    return this.statusOptions.find(x => x.value === status)?.label ?? `Status ${status}`;
  }

  trackByProject(_: number, item: ProjectDto): string {
    return item.id;
  }

  get maxPageIndex(): number {
    return Math.max(0, Math.ceil(this.totalCount / this.pageSize) - 1);
  }

  private todayDate(): string {
    return new Date().toISOString().slice(0, 10);
  }
}


