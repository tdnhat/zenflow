"use client";

import React from "react";
import { DashboardSidebar } from "./sidebar";

interface DashboardLayoutProps {
  children: React.ReactNode;
}

/**
 * Main dashboard layout component
 * Contains the sidebar, header, and main content area
 */
export function DashboardLayout({ children }: DashboardLayoutProps) {
  return (
    // <div className="flex h-screen overflow-hidden bg-background">
      <DashboardSidebar>{children}</DashboardSidebar>
    // </div>
  );
} 