import Styles from "./TopBar.module.css";
import { Button } from "./Button.tsx";

export function TopBar() {
    return (
        <div className={Styles.root}>
            <Button className={Styles.userButton}>Log In</Button>
        </div>
    );
}
