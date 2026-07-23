import { useRef, useState } from "react";
import { useClickOutside } from "../hooks/useClickOutside";

type PaginationProps = {
  totalItems: number;
  page: number;
  totalPages: number;
  pageSize: number;
  pageSizeOptions?: readonly number[];
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  ariaLabel?: string;
};

export function Pagination({
  totalItems,
  page,
  totalPages,
  pageSize,
  pageSizeOptions = [10, 25, 50, 100],
  onPageChange,
  onPageSizeChange,
  ariaLabel = "Pagination"
}: PaginationProps) {
  const [isPageSizeMenuOpen, setIsPageSizeMenuOpen] = useState(false);
  const pageSizeMenuRef = useRef<HTMLDivElement | null>(null);
  const currentPage = Math.max(1, Math.min(page, Math.max(1, totalPages)));

  useClickOutside(
    [pageSizeMenuRef],
    () => setIsPageSizeMenuOpen(false),
    isPageSizeMenuOpen
  );

  return (
    <div className="admin-pagination">
      <div className="pagination-summary">
        <span>{totalItems} items</span>
        <span>Page {currentPage} of {Math.max(1, totalPages)}</span>
      </div>
      <div className="pagination-controls">
        <div className="pagination-page-size" ref={pageSizeMenuRef}>
          <button
            type="button"
            className="pagination-page-size-trigger"
            aria-expanded={isPageSizeMenuOpen}
            aria-haspopup="listbox"
            onClick={() => setIsPageSizeMenuOpen((current) => !current)}
          >
            {pageSize}
          </button>
          <span>/ page</span>
          {isPageSizeMenuOpen ? (
            <div className="pagination-page-size-menu" role="listbox">
              {pageSizeOptions.map((size) => (
                <button
                  key={size}
                  type="button"
                  className={size === pageSize ? "pagination-page-size-option pagination-page-size-option-active" : "pagination-page-size-option"}
                  role="option"
                  aria-selected={size === pageSize}
                  onClick={() => {
                    onPageSizeChange(size);
                    setIsPageSizeMenuOpen(false);
                  }}
                >
                  {size}
                </button>
              ))}
            </div>
          ) : null}
        </div>
        {totalPages > 1 ? (
          <div className="pagination-pages" aria-label={ariaLabel}>
            {currentPage > 1 ? <button type="button" className="pagination-arrow pagination-arrow-prev" aria-label="Previous page" onClick={() => onPageChange(currentPage - 1)} /> : null}
            {getVisiblePages(currentPage, totalPages).map((item, index) => item === "gap"
              ? <span key={`gap-${index}`} className="pagination-gap">...</span>
              : <button key={item} type="button" className={item === currentPage ? "pagination-page pagination-page-active" : "pagination-page"} onClick={() => onPageChange(item)}>{item}</button>)}
            {currentPage < totalPages ? <button type="button" className="pagination-arrow pagination-arrow-next" aria-label="Next page" onClick={() => onPageChange(currentPage + 1)} /> : null}
          </div>
        ) : null}
      </div>
    </div>
  );
}

function getVisiblePages(currentPage: number, totalPages: number): Array<number | "gap"> {
  if (totalPages <= 7) return Array.from({ length: totalPages }, (_, index) => index + 1);
  if (currentPage <= 4) return [1, 2, 3, 4, 5, "gap", totalPages];
  if (currentPage >= totalPages - 3) return [1, "gap", totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
  return [1, "gap", currentPage - 1, currentPage, currentPage + 1, "gap", totalPages];
}
