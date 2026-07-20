import { useEffect, useState, type FormEvent } from "react";
import { Button } from "../../../shared/components/ui/button";
import { Input } from "../../../shared/components/ui/input";
import type { ShortLinkDiscoveryQuery } from "../types";

export const defaultShortLinkDiscoveryQuery: ShortLinkDiscoveryQuery = {
  search: "",
  status: "all",
  sortBy: "created",
  sortDirection: "desc"
};

export function hasShortLinkDiscoveryCriteria(query: ShortLinkDiscoveryQuery) {
  return query.search.trim() !== ""
    || query.status !== defaultShortLinkDiscoveryQuery.status
    || query.sortBy !== defaultShortLinkDiscoveryQuery.sortBy
    || query.sortDirection !== defaultShortLinkDiscoveryQuery.sortDirection;
}

export function createShortLinkDiscoveryChange(query: ShortLinkDiscoveryQuery) {
  return { query, pageNumber: 1 } as const;
}

type ShortLinkDiscoveryToolbarProps = {
  value: ShortLinkDiscoveryQuery;
  disabled?: boolean;
  onChange: (value: ShortLinkDiscoveryQuery) => void;
};

export function ShortLinkDiscoveryToolbar({
  value,
  disabled = false,
  onChange
}: ShortLinkDiscoveryToolbarProps) {
  const [search, setSearch] = useState(value.search);

  useEffect(() => setSearch(value.search), [value.search]);

  const submitSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    onChange({ ...value, search: search.trim() });
  };

  return (
    <form className="admin-discovery-toolbar" aria-label="Find and sort short links" onSubmit={submitSearch}>
      <div className="admin-discovery-search">
        <Input
          value={search}
          disabled={disabled}
          aria-label="Search code or destination"
          placeholder="Search code or destination"
          onChange={(event) => setSearch(event.target.value)}
        />
        <Button type="submit" variant="secondary" disabled={disabled}>
          Search
        </Button>
      </div>

      <label className="admin-discovery-field">
        <span>Status</span>
        <select
          value={value.status}
          disabled={disabled}
          onChange={(event) => onChange({
            ...value,
            status: event.target.value as ShortLinkDiscoveryQuery["status"]
          })}
        >
          <option value="all">All</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
          <option value="expired">Expired</option>
          <option value="expiring-soon">Expiring soon</option>
        </select>
      </label>

      <label className="admin-discovery-field">
        <span>Sort by</span>
        <select
          value={value.sortBy}
          disabled={disabled}
          onChange={(event) => onChange({
            ...value,
            sortBy: event.target.value as ShortLinkDiscoveryQuery["sortBy"]
          })}
        >
          <option value="created">Created date</option>
          <option value="expiry">Expiry</option>
          <option value="destination">Destination</option>
          <option value="code">Code</option>
          <option value="status">Status</option>
        </select>
      </label>

      <label className="admin-discovery-field">
        <span>Direction</span>
        <select
          value={value.sortDirection}
          disabled={disabled}
          onChange={(event) => onChange({
            ...value,
            sortDirection: event.target.value as ShortLinkDiscoveryQuery["sortDirection"]
          })}
        >
          <option value="desc">Descending</option>
          <option value="asc">Ascending</option>
        </select>
      </label>

      {hasShortLinkDiscoveryCriteria(value) || search !== value.search ? (
        <Button
          type="button"
          variant="secondary"
          disabled={disabled}
          onClick={() => onChange(defaultShortLinkDiscoveryQuery)}
        >
          Reset
        </Button>
      ) : null}
    </form>
  );
}
