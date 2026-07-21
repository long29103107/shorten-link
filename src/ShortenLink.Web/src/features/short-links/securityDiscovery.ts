import type { SecurityUser } from "./types";

export type SecurityUserStatusFilter = "all" | "enabled" | "disabled";
export type SecurityUserSortField = "email" | "displayName" | "createdAt";
export type SortDirection = "asc" | "desc";

export type SecurityUserDiscovery = {
  search: string;
  status: SecurityUserStatusFilter;
  sortBy: SecurityUserSortField;
  direction: SortDirection;
};

export const defaultSecurityUserDiscovery: SecurityUserDiscovery = {
  search: "",
  status: "all",
  sortBy: "createdAt",
  direction: "desc"
};

export function discoverSecurityUsers(users: SecurityUser[], query: SecurityUserDiscovery): SecurityUser[] {
  const search = query.search.trim().toLocaleLowerCase();
  const filtered = users.filter((user) => {
    const matchesSearch = !search
      || user.username.toLocaleLowerCase().includes(search)
      || user.displayName.toLocaleLowerCase().includes(search);
    const matchesStatus = query.status === "all"
      || (query.status === "enabled" ? user.isEnabled : !user.isEnabled);
    return matchesSearch && matchesStatus;
  });

  return filtered.sort((left, right) => {
    const leftValue = query.sortBy === "email"
      ? left.username
      : query.sortBy === "displayName"
        ? left.displayName
        : left.createdAtUtc;
    const rightValue = query.sortBy === "email"
      ? right.username
      : query.sortBy === "displayName"
        ? right.displayName
        : right.createdAtUtc;
    const result = leftValue.localeCompare(rightValue);
    return query.direction === "asc" ? result : -result;
  });
}

export function paginateItems<T>(items: T[], page: number, pageSize: number): T[] {
  return items.slice((page - 1) * pageSize, page * pageSize);
}

export function getVisiblePages(currentPage: number, totalPages: number): Array<number | "gap"> {
  if (totalPages <= 7) return Array.from({ length: totalPages }, (_, index) => index + 1);
  const pages = new Set([1, totalPages, currentPage - 1, currentPage, currentPage + 1]);
  const visible = [...pages].filter((page) => page >= 1 && page <= totalPages).sort((a, b) => a - b);
  const result: Array<number | "gap"> = [];
  visible.forEach((page, index) => {
    if (index > 0 && page - visible[index - 1] > 1) result.push("gap");
    result.push(page);
  });
  return result;
}
