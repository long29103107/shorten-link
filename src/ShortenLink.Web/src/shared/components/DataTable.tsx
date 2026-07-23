import type { ReactNode, TdHTMLAttributes, ThHTMLAttributes } from "react";
import { Button } from "./ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "./ui/table";

export type DataTableColumn<TRow> = {
  id: string;
  header: ReactNode;
  cell: (row: TRow) => ReactNode;
  headProps?: ThHTMLAttributes<HTMLTableCellElement>;
  cellProps?: TdHTMLAttributes<HTMLTableCellElement>;
};

export type DataTableBulkAction = {
  id: string;
  label: ReactNode | ((selectedCount: number) => ReactNode);
  variant?: "default" | "secondary" | "destructive" | "ghost";
  disabled?: boolean;
  onSelect: () => void;
};

type DataTableProps<TRow> = {
  rows: readonly TRow[];
  columns: readonly DataTableColumn<TRow>[];
  getRowKey: (row: TRow) => string;
  ariaLabel?: string;
  bulkSelection?: {
    selectedKeys: ReadonlySet<string>;
    onChange: (selectedKeys: Set<string>) => void;
    actions: readonly DataTableBulkAction[];
    getRowLabel?: (row: TRow) => string;
    clearDisabled?: boolean;
  };
};

export function DataTable<TRow>({
  rows,
  columns,
  getRowKey,
  ariaLabel,
  bulkSelection
}: DataTableProps<TRow>) {
  const pageKeys = rows.map(getRowKey);
  const selectedCount = bulkSelection?.selectedKeys.size ?? 0;
  const isPageSelected = pageKeys.length > 0 && pageKeys.every((key) => bulkSelection?.selectedKeys.has(key));
  const togglePage = () => {
    if (!bulkSelection) return;
    const next = new Set(bulkSelection.selectedKeys);
    if (isPageSelected) pageKeys.forEach((key) => next.delete(key));
    else pageKeys.forEach((key) => next.add(key));
    bulkSelection.onChange(next);
  };
  const toggleRow = (key: string) => {
    if (!bulkSelection) return;
    const next = new Set(bulkSelection.selectedKeys);
    if (next.has(key)) next.delete(key);
    else next.add(key);
    bulkSelection.onChange(next);
  };

  return (
    <>
      {bulkSelection && selectedCount > 0 && bulkSelection.actions.length > 0 ? (
        <div className="admin-bulk-bar">
          <div className="admin-toolbar-group">
            {bulkSelection.actions.map((action) => (
              <Button key={action.id} variant={action.variant ?? "secondary"} disabled={action.disabled} onClick={action.onSelect}>
                {typeof action.label === "function" ? action.label(selectedCount) : action.label}
              </Button>
            ))}
            <Button variant="secondary" disabled={bulkSelection.clearDisabled} onClick={() => bulkSelection.onChange(new Set())}>Clear selected</Button>
          </div>
        </div>
      ) : null}
      <div className="admin-table-wrap">
        <Table aria-label={ariaLabel}>
        <TableHeader>
          <TableRow>
            {bulkSelection ? (
              <TableHead>
                <input type="checkbox" className="bulk-checkbox" checked={isPageSelected} disabled={rows.length === 0} aria-label={`Select all ${ariaLabel ?? "rows"} on this page`} onChange={togglePage} />
              </TableHead>
            ) : null}
            {columns.map((column) => (
              <TableHead key={column.id} {...column.headProps}>
                {column.header}
              </TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={getRowKey(row)}>
              {bulkSelection ? (
                <TableCell>
                  <input type="checkbox" className="bulk-checkbox" checked={bulkSelection.selectedKeys.has(getRowKey(row))} aria-label={bulkSelection.getRowLabel?.(row) ?? `Select ${getRowKey(row)}`} onChange={() => toggleRow(getRowKey(row))} />
                </TableCell>
              ) : null}
              {columns.map((column) => (
                <TableCell key={column.id} {...column.cellProps}>
                  {column.cell(row)}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
      </div>
    </>
  );
}
