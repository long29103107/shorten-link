import { CreateShortLinkForm } from "../components/CreateShortLinkForm";
import { RecentLinkPanel } from "../components/RecentLinkPanel";
import type { CreatedShortLink } from "../types";

type CreateShortLinkPageProps = {
  recentLink: CreatedShortLink | null;
  onCreated: (createdLink: CreatedShortLink) => void;
};

export function CreateShortLinkPage({
  recentLink,
  onCreated
}: CreateShortLinkPageProps) {
  return (
    <div className="workspace-grid">
      <CreateShortLinkForm onCreated={onCreated} />
      <RecentLinkPanel recentLink={recentLink} />
    </div>
  );
}
