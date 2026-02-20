export interface PagedRequestDto {
  skipCount: number;
  maxResultCount: number;
  sorting?: string;
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}
