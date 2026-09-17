import { useEffect, useRef, useState } from "react";
import { useIsFetching, useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import type { AboutDto } from "../api/types";

const MIN_MS = 420;
const MAX_MS = 8000;

export function useSiteBoot() {
  const fetching = useIsFetching();
  const about = useQuery({
    queryKey: ["about"],
    queryFn: () => api<AboutDto>("/api/about")
  });
  const [ready, setReady] = useState(false);
  const started = useRef(Date.now());

  useEffect(() => {
    if (ready) {
      return;
    }

    if (about.isPending || fetching > 0) {
      return;
    }

    const wait = Math.max(0, MIN_MS - (Date.now() - started.current));
    const id = window.setTimeout(() => setReady(true), wait);
    return () => window.clearTimeout(id);
  }, [ready, about.isPending, fetching]);

  useEffect(() => {
    if (ready) {
      return;
    }

    const id = window.setTimeout(() => setReady(true), MAX_MS);
    return () => window.clearTimeout(id);
  }, [ready]);

  return !ready;
}
