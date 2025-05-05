import { DesktopNavLink } from "./nav-container";

export interface NavLink {
    label: string;
    href: string;
    icon: React.ReactNode;
}

export const NavLinks = ({
    links,
    isExpanded,
}: {
    links: NavLink[];
    isExpanded: boolean;
}) => {
    return (
        <div className="mt-8 flex flex-col gap-2">
            {links.map((link, idx) => (
                <DesktopNavLink key={idx} link={link} isExpanded={isExpanded} />
            ))}
        </div>
    );
};


