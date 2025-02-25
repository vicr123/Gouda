import { Dialog as BaseDialog } from "@base-ui-components/react";
import { ComponentProps } from "react";
import Styles from "./Dialog.module.css";

export const Dialog = {
    Root: ({ ...props }: ComponentProps<typeof BaseDialog.Root>) => (
        <BaseDialog.Root {...props} />
    ),
    Trigger: ({ ...props }: ComponentProps<typeof BaseDialog.Trigger>) => (
        <BaseDialog.Trigger {...props} />
    ),
    Portal: ({ ...props }: ComponentProps<typeof BaseDialog.Portal>) => (
        <BaseDialog.Portal {...props} />
    ),
    Backdrop: ({
        className,
        ...props
    }: ComponentProps<typeof BaseDialog.Backdrop>) => (
        <BaseDialog.Backdrop
            {...props}
            className={[Styles.backdrop, className].join(" ")}
        />
    ),
    Popup: ({
        className,
        ...props
    }: ComponentProps<typeof BaseDialog.Popup>) => (
        <BaseDialog.Popup
            {...props}
            className={[Styles.popup, className].join(" ")}
        />
    ),
    Title: ({
        className,
        ...props
    }: ComponentProps<typeof BaseDialog.Title>) => (
        <BaseDialog.Title
            {...props}
            className={[Styles.title, className].join(" ")}
        />
    ),
    Description: ({
        ...props
    }: ComponentProps<typeof BaseDialog.Description>) => (
        <BaseDialog.Description {...props} />
    ),
    Close: ({ ...props }: ComponentProps<typeof BaseDialog.Close>) => (
        <BaseDialog.Close {...props} />
    ),
};
