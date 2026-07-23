import { Button } from "./ui/button";

type RefreshButtonProps = {
  isRefreshing: boolean;
  onRefresh: () => void | Promise<void>;
  label?: string;
  showLabel?: boolean;
};

export function RefreshButton({
  isRefreshing,
  onRefresh,
  label = "Refresh",
  showLabel = false
}: RefreshButtonProps) {
  return (
    <Button
      className={showLabel ? undefined : "icon-only-button"}
      variant="secondary"
      disabled={isRefreshing}
      aria-label={label}
      title={label}
      onClick={() => void onRefresh()}
    >
      <svg
        className={isRefreshing ? "refresh-icon refresh-icon-spinning" : "refresh-icon"}
        viewBox="0 0 24 24"
        aria-hidden="true"
      >
        <path d="M20 6v5h-5" />
        <path d="M4 18v-5h5" />
        <path d="M6.1 9a7 7 0 0 1 11.5-2.6L20 9" />
        <path d="M17.9 15a7 7 0 0 1-11.5 2.6L4 15" />
      </svg>
      {showLabel ? (isRefreshing ? "Refreshing" : label) : null}
    </Button>
  );
}
