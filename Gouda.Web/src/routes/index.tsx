import { createFileRoute } from "@tanstack/react-router";
import Styles from "./index.module.css";

export const Route = createFileRoute("/")({
    component: About,
});

function About() {
    return (
        <div className={Styles.root}>
            <h1 className={Styles.title}>Gouda</h1>
            <div className={Styles.text}>A small spiel about Gouda</div>
        </div>
    );
}
