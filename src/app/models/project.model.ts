export interface ProjectDto {
  id: number;
  name: string;
  status: string;
  startDate: string;
  endDate?: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalPages: number;
  totalCount: number;
}
