import React, { createContext, useContext, useState } from 'react';

const SidebarContext = createContext();

export function SidebarProvider({ children }) {
    const [isCollapsed, setIsCollapsed] = useState(true);

    const toggleSidebar = () => {
        setIsCollapsed(prev => !prev);
    };

    return (
        <SidebarContext.Provider value={{ isCollapsed, toggleSidebar }}>
            {children}
        </SidebarContext.Provider>
    );
}

export function useSidebar() {
    const context = useContext(SidebarContext);
    if (context === undefined) {
        throw new Error('useSidebar must be used within a SidebarProvider');
    }
    return context;
}
