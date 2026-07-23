import { useEffect, useState } from "react";
import { listSecurityRoles, listSecurityUsers, listShortLinks } from "../api/shortLinksApi";
import { Card, CardContent } from "../../../shared/components/ui/card";
import { RefreshButton } from "../../../shared/components/RefreshButton";

type DashboardSnapshot = {
  totalLinks: number;
  activeLinks: number;
  users: number;
  roles: number;
};

export function AdminDashboardPage() {
  const [snapshot, setSnapshot] = useState<DashboardSnapshot | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadDashboard = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [links, users, roles] = await Promise.all([
        listShortLinks(100, 1),
        listSecurityUsers(),
        listSecurityRoles()
      ]);
      setSnapshot({
        totalLinks: links.totalCount ?? links.items.length,
        activeLinks: links.items.filter((link) => link.isActive).length,
        users: users.items.length,
        roles: roles.systemRoles.length + roles.customRoles.length
      });
    } catch {
      setError("Dashboard data could not be loaded.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadDashboard();
  }, []);

  return (
    <>
      <nav className="page-breadcrumb-bar" aria-label="Breadcrumb">
        <ol className="page-breadcrumb">
          <li>Shorten Link</li>
          <li aria-current="page">Dashboard</li>
        </ol>
        <RefreshButton isRefreshing={isLoading} label="Refresh dashboard data" onRefresh={loadDashboard} />
      </nav>

      <div className="dashboard-grid">
        <DashboardMetric label="Total short links" value={snapshot?.totalLinks} loading={isLoading} />
        <DashboardMetric label="Active in latest 100" value={snapshot?.activeLinks} loading={isLoading} />
        <DashboardMetric label="Managed users" value={snapshot?.users} loading={isLoading} />
        <DashboardMetric label="Available roles" value={snapshot?.roles} loading={isLoading} />
        <Card className="dashboard-health-card">
          <CardContent>
            <p className="eyebrow">System health</p>
            <h2>{error ? "Attention required" : isLoading ? "Checking" : "Operational"}</h2>
            <p className="muted-copy">
              {error ?? "Short-link, identity, and role data are responding normally."}
            </p>
          </CardContent>
        </Card>
      </div>
    </>
  );
}

function DashboardMetric({ label, value, loading }: { label: string; value?: number; loading: boolean }) {
  return (
    <Card className="dashboard-metric-card">
      <CardContent>
        <span>{label}</span>
        <strong>{loading ? "—" : value ?? 0}</strong>
      </CardContent>
    </Card>
  );
}
