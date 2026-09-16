import type { ReactNode } from "react";

export function Note({ children }: { children: ReactNode }) {
  return <p className="muted">{children}</p>;
}

export function SkeletonRows({ count = 3 }: { count?: number }) {
  return (
    <div aria-hidden="true">
      {Array.from({ length: count }).map((_, index) => (
        <div key={index} style={{ display: "grid", gap: 8, padding: "18px 0" }}>
          <div className="skeleton" style={{ width: "40%" }} />
          <div className="skeleton" style={{ width: "78%" }} />
        </div>
      ))}
    </div>
  );
}
