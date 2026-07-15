type SkeletonProps = {
  className?: string;
};

export function Skeleton({ className }: SkeletonProps) {
  return <span className={className ? `skeleton ${className}` : "skeleton"} />;
}
