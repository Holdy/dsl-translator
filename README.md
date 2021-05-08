# dsl-translator

## A lightweight translator for domain specific languages.

Given a statement in a domain specific language :

```
1: each active customer (cust) where
2: (cust) placed an order (order_1)
3: (order_1) total was between £10 and £100
```

A translation can be generated...

```sql
  FROM some_base_schema.customer cust                              -- line 1
  JOIN order_schema.order order_1 ON order_1.customer_pk = cust.pk -- line 2
 WHERE cust.soft_deleted_date IS NULL                              -- line 1
   AND order_1.total_order_amount BETWEEN 10 AND 100               -- line 3
```

...ready for an implementation specific `SELECT` or `INSERT` clause to be added.

## Defining your Domain Specific Language

The DSL for line 1 above is held in a file where the file name defines the pattern:

`each active customer (customer alias) where`.dsl

The `(customer alias)` placeholder tells the translation engine that the user will provide a new alias, like (cust), here and the type of that alias is 'customer' - so (cust) can't later be used where an 'order' reference is required.

The file content gives the translation:
```
from  : FROM some_base_schema.customer (0)
where : (0).soft_deleted_date IS NULL
```

During processing, placeholders in the translation (0) are substituted with input from the source language.

The typing of alias lets the language contain otherwise identical phrases that have different implementations E.g:

```
(customer reference) is over (number literal) days old
   => where : (0).days_since_signup > (1)

(order reference) is over (number literal) days old
   => where : (0).date_ordered < current_date - (1)
```

## Finishing Touches
The translated lines are collected by type ('from', 'join', 'where'). The final step combines them in the right order and adds supporting syntax e.g. inserting `AND` between 'where' clauses.