"use client";

import React from "react";
import { Bell, Search, Menu } from "lucide-react";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { Button } from "@/components/ui/button";

interface HeaderProps {
  onOpenSidebar: () => void;
}

/**
 * Application header with mobile menu toggle, search, and user menu
 */
export function Header({ onOpenSidebar }: HeaderProps) {
  return (
    <header className="h-16 flex items-center justify-between px-4 md:px-6 border-b border-border bg-card">
      {/* Mobile menu button */}
      <Button 
        variant="ghost" 
        size="icon" 
        className="md:hidden text-card-foreground"
        onClick={onOpenSidebar}
      >
        <Menu className="h-5 w-5" />
        <span className="sr-only">Toggle menu</span>
      </Button>
      
      {/* Search */}
      <div className="hidden md:flex items-center w-full max-w-sm relative">
        <Search className="absolute left-2.5 h-4 w-4 text-muted-foreground" />
        <input
          type="search"
          placeholder="Search..."
          className="flex h-10 w-full rounded-md border border-input bg-card/50 px-3 py-2 pl-8 text-sm text-card-foreground placeholder:text-muted-foreground"
        />
      </div>
      
      {/* Right side actions */}
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="icon" className="relative text-card-foreground">
          <Bell className="h-5 w-5" />
          <span className="absolute top-1 right-1 h-2 w-2 rounded-full bg-primary"></span>
          <span className="sr-only">Notifications</span>
        </Button>
        
        <ThemeToggle />
        
        {/* User menu */}
        <Button variant="ghost" size="icon" className="rounded-full h-8 w-8 border border-border bg-primary/10">
          <span className="font-semibold text-xs text-primary">ZF</span>
          <span className="sr-only">User menu</span>
        </Button>
      </div>
    </header>
  );
} 