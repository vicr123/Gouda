import { ComponentProps } from "react";
import { Select as BaseSelect } from "@base-ui-components/react";
import Styles from "./Select.module.css";

export const Select = {
    Root: <T,>({ ...props }: ComponentProps<typeof BaseSelect.Root<T>>) => (
        <BaseSelect.Root<T> {...props} />
    ),
    Trigger: ({
        className,
        ...props
    }: ComponentProps<typeof BaseSelect.Trigger>) => (
        <BaseSelect.Trigger
            {...props}
            className={[Styles.trigger, className].join(" ")}
        />
    ),
    Value: ({
        className,
        ...props
    }: ComponentProps<typeof BaseSelect.Value>) => (
        <BaseSelect.Value
            {...props}
            className={[Styles.value, className].join(" ")}
        />
    ),
    Icon: ({ className, ...props }: ComponentProps<typeof BaseSelect.Icon>) => (
        <BaseSelect.Icon
            {...props}
            className={[Styles.icon, className].join(" ")}
        />
    ),
    Portal: ({ ...props }: ComponentProps<typeof BaseSelect.Portal>) => (
        <BaseSelect.Portal {...props} />
    ),
    Backdrop: ({ ...props }: ComponentProps<typeof BaseSelect.Backdrop>) => (
        <BaseSelect.Backdrop {...props} />
    ),
    Positioner: ({
        ...props
    }: ComponentProps<typeof BaseSelect.Positioner>) => (
        <BaseSelect.Positioner {...props} />
    ),
    ScrollUpArrow: ({
        ...props
    }: ComponentProps<typeof BaseSelect.ScrollUpArrow>) => (
        <BaseSelect.ScrollUpArrow {...props} />
    ),
    Popup: ({
        className,
        ...props
    }: ComponentProps<typeof BaseSelect.Popup>) => (
        <BaseSelect.Popup
            {...props}
            className={[Styles.popup, className].join(" ")}
        />
    ),
    Arrow: ({ ...props }: ComponentProps<typeof BaseSelect.Arrow>) => (
        <BaseSelect.Arrow {...props} />
    ),
    Item: ({ className, ...props }: ComponentProps<typeof BaseSelect.Item>) => (
        <BaseSelect.Item
            {...props}
            className={[Styles.item, className].join(" ")}
        />
    ),
    CategoryHeader: ({ className, ...props }: ComponentProps<"div">) => (
        <div
            {...props}
            className={[Styles.categoryHeader, className].join(" ")}
        />
    ),
    ItemText: ({
        className,
        ...props
    }: ComponentProps<typeof BaseSelect.ItemText>) => (
        <BaseSelect.ItemText
            {...props}
            className={[Styles.itemText, className].join(" ")}
        />
    ),
    ItemIndicator: ({
        className,
        ...props
    }: ComponentProps<typeof BaseSelect.ItemIndicator>) => (
        <BaseSelect.ItemIndicator
            {...props}
            className={[Styles.itemIndicator, className].join(" ")}
        />
    ),
    Separator: ({ ...props }: ComponentProps<typeof BaseSelect.Separator>) => (
        <BaseSelect.Separator {...props} />
    ),
    Group: ({ ...props }: ComponentProps<typeof BaseSelect.Group>) => (
        <BaseSelect.Group {...props} />
    ),
    GroupLabel: ({
        ...props
    }: ComponentProps<typeof BaseSelect.GroupLabel>) => (
        <BaseSelect.GroupLabel {...props} />
    ),
    ScrollDownArrow: ({
        ...props
    }: ComponentProps<typeof BaseSelect.ScrollDownArrow>) => (
        <BaseSelect.ScrollDownArrow {...props} />
    ),
};
