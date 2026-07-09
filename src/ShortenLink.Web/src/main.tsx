import React from "react";
import { createRoot } from "react-dom/client";
import "./styles.css";

function App() {
  return (
    <main className="app-shell">
      <section>
        <p className="eyebrow">Shorten Link Demo</p>
        <h1>Reusable .NET short-link library</h1>
        <p>
          This frontend scaffold is ready for the Phase 1 create-link flow. The
          business behavior will stay in the reusable library and demo API.
        </p>
      </section>
    </main>
  );
}

createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
