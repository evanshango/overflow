'use client'

import React from 'react';
import {Tag} from '@/lib/types';
import {Card, CardBody, CardFooter, CardHeader} from '@heroui/card';
import Link from 'next/link';
import {Chip} from '@heroui/chip';

type Props = {
    tag: Tag
}

const TagCard = ({tag}: Props) => {
    return (
        <Card as={Link} href={`/questions?tag=${tag.slug}`} isHoverable isPressable>
            <CardHeader>
                <Chip variant="bordered">
                    {tag.slug}
                </Chip>
            </CardHeader>
            <CardBody>
                <p className='line-clamp-3'>
                    {tag.description}
                </p>
            </CardBody>
            <CardFooter>
                42 questions
            </CardFooter>
        </Card>
    );
};

export default TagCard;