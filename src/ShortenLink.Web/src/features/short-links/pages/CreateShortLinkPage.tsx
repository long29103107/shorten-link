import { CreateShortLinkForm } from "../components/CreateShortLinkForm";
import { RecentLinkPanel } from "../components/RecentLinkPanel";
import type { CreatedShortLink } from "../types";

type CreateShortLinkPageProps = {
  recentLink: CreatedShortLink | null;
  onCreated: (createdLink: CreatedShortLink) => void;
  onOpenDetails: (code: string) => void;
};

export function CreateShortLinkPage({
  recentLink,
  onCreated,
  onOpenDetails
}: CreateShortLinkPageProps) {
  return (
    <div className="workspace-grid">
      <CreateShortLinkForm onCreated={onCreated} onOpenDetails={onOpenDetails} />
      <RecentLinkPanel recentLink={recentLink} onOpenDetails={onOpenDetails} />
    </div>
  );
}
