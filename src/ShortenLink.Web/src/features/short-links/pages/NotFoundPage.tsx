type NotFoundPageProps = {
  onBackHome: () => void;
};

export function NotFoundPage({ onBackHome }: NotFoundPageProps) {
  return (
    <section className="panel panel-detail">
      <div className="panel-heading">
        <p className="eyebrow">Fallback</p>
        <h2>This route is not part of the demo flow.</h2>
      </div>
      <p className="muted-copy">
        Try creating a new short link or open a detail route in the form
        <code> /links/your-code</code>.
      </p>
      <div className="form-actions">
        <button className="action-button" type="button" onClick={onBackHome}>
          Return to create flow
        </button>
      </div>
    </section>
  );
}
