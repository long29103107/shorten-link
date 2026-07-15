import { Skeleton } from "./Skeleton";

type TableSkeletonProps = {
  rows?: number;
};

export function TableSkeleton({ rows = 6 }: TableSkeletonProps) {
  return (
    <div className="table-skeleton" aria-label="Loading data">
      <div className="table-skeleton-toolbar">
        <Skeleton className="skeleton-button" />
        <Skeleton className="skeleton-button skeleton-button-short" />
      </div>
      <div className="table-skeleton-grid">
        {Array.from({ length: rows }).map((_, index) => (
          <div key={index} className="table-skeleton-row">
            <Skeleton className="skeleton-check" />
            <Skeleton className="skeleton-code" />
            <Skeleton className="skeleton-url" />
            <Skeleton className="skeleton-date" />
            <Skeleton className="skeleton-date" />
            <Skeleton className="skeleton-status" />
            <Skeleton className="skeleton-actions" />
          </div>
        ))}
      </div>
    </div>
  );
}
