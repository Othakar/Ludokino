"use client";

import { useEffect, useState } from "react";
import { usePathname, useRouter } from "next/navigation";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const [authReady, setAuthReady] = useState(false);

  useEffect(() => {
    const publicRoutes = ["/admin/login"];
    const token = localStorage.getItem("ludokino_token");

    if (publicRoutes.includes(pathname)) {
      setAuthReady(true);
      return;
    }

    if (!token) {
      router.replace("/admin/login");
      return;
    }

    setAuthReady(true);
  }, [pathname, router]);

  if (!authReady) {
    return null;
  }

  return <>{children}</>;
}
