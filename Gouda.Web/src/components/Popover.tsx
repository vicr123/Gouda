import { Popover as BasePopover } from "@base-ui-components/react";
import { ComponentProps } from "react";
import Styles from "./Popover.module.css";

export const Popover = {
    Root: ({ ...props }: ComponentProps<typeof BasePopover.Root>) => (
        <BasePopover.Root {...props} />
    ),
    Trigger: ({ ...props }: ComponentProps<typeof BasePopover.Trigger>) => (
        <BasePopover.Trigger {...props} />
    ),
    Portal: ({ ...props }: ComponentProps<typeof BasePopover.Portal>) => (
        <BasePopover.Portal {...props} />
    ),
    Positioner: ({
        ...props
    }: ComponentProps<typeof BasePopover.Positioner>) => (
        <BasePopover.Positioner {...props} />
    ),
    Popup: ({
        className,
        ...props
    }: ComponentProps<typeof BasePopover.Popup>) => (
        <BasePopover.Popup
            {...props}
            className={[Styles.popup, className].join(" ")}
        />
    ),
    Title: ({ ...props }: ComponentProps<typeof BasePopover.Title>) => (
        <BasePopover.Title {...props} />
    ),
    Description: ({
        ...props
    }: ComponentProps<typeof BasePopover.Description>) => (
        <BasePopover.Description {...props} />
    ),
};
