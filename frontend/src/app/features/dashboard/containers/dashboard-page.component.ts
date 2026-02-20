import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { SecurityContext } from '@angular/core';
import { DashboardService } from '../../../proxy/dashboard/dashboard.service';
import { DashboardStatsDto } from '../../../proxy/models';

@Component({
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPageComponent implements OnInit {
  isLoading = true;
  hasError = false;
  stats: DashboardStatsDto | null = null;
  reportSummaryHtml = '';

  constructor(
    private readonly dashboardService: DashboardService,
    private readonly sanitizer: DomSanitizer,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.reportSummaryHtml =
      this.sanitizer.sanitize(SecurityContext.HTML, '<strong>Revenue trend:</strong> waiting for data') ?? '';
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.hasError = false;

    this.dashboardService.getStats().subscribe({
      next: stats => {
        this.stats = stats;
        this.reportSummaryHtml =
          this.sanitizer.sanitize(
            SecurityContext.HTML,
            `<strong>Revenue trend:</strong> ${stats.revenueByMonth.length} monthly points`
          ) ?? '';
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

  trackByMetric(index: number): number {
    return index;
  }
}



