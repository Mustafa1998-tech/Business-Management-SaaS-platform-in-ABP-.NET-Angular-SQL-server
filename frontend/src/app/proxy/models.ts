export interface CustomerDto {
  id: string;
  name: string;
  email: string;
  phone: string;
  address: string;
  isActive: boolean;
  creationTime?: string;
}

export interface CreateUpdateCustomerDto {
  name: string;
  email: string;
  phone: string;
  address: string;
  isActive: boolean;
}

export interface CustomerListRequestDto {
  skipCount: number;
  maxResultCount: number;
  sorting?: string;
  filter?: string;
  isActive?: boolean;
}

export interface DashboardStatsDto {
  totalCustomers: number;
  activeProjects: number;
  pendingTasks: number;
  outstandingInvoices: number;
  monthRevenue: number;
  revenueByMonth: RevenuePointDto[];
}

export interface RevenuePointDto {
  month: string;
  amount: number;
}

export interface KanbanTaskCardDto {
  id: string;
  projectId: string;
  title: string;
  dueDate?: string;
  sortOrder: number;
}

export interface KanbanColumnDto {
  status: number;
  label: string;
  tasks: KanbanTaskCardDto[];
}

export interface MoveTaskDto {
  taskId: string;
  newStatus: number;
  newOrder: number;
}

export interface InvoiceReportFilterDto {
  fromDate?: string;
  toDate?: string;
  customerId?: string;
  status?: number;
}

export interface InvoiceReportSummaryDto {
  totalRevenue: number;
  totalInvoices: number;
  paidInvoices: number;
  pendingInvoices: number;
  overdueInvoices: number;
}

export interface InvoiceReportDto {
  invoiceNo: string;
  customerName: string;
  projectName: string;
  amount: number;
  status: number;
  date: string;
}

export interface InvoicesReportResultDto {
  summary: InvoiceReportSummaryDto;
  items: InvoiceReportDto[];
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}
