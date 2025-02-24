import Styles from "./Sidebar.module.css";
import { ReactNode } from "react";
import { Separator } from "./Separator.tsx";

export function Sidebar() {
    return (
        <div className={Styles.sidebar}>
            <SidebarItem>Gouda</SidebarItem>
            <Separator />
            <SidebarItem>User Settings</SidebarItem>
            <SidebarItem>Portable Pins</SidebarItem>
        </div>
    );
}

export function SidebarItem({ children }: { children: ReactNode }) {
    return <div className={Styles.sidebarItem}>{children}</div>;
}
