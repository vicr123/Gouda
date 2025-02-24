import { ReactNode } from "react";
import Styles from "./PageLayout.module.css";

export function PageLayout({ children }: { children: ReactNode }) {
    return <div className={Styles.page}>{children}</div>;
}
