type EmptyStateProps = {
  title: string;
  description?: string;
};

export function EmptyState({ title, description }: EmptyStateProps) {
  return (
    <div className="empty-state">
      <div className="empty-state-icon" aria-hidden="true">
        <span />
      </div>
      <div>
        <p className="empty-state-title">{title}</p>
        {description ? <p className="empty-state-description">{description}</p> : null}
      </div>
    </div>
  );
}
